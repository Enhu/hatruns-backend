using AutoMapper;
using HatCommunityWebsite.DB;
using HatCommunityWebsite.Repo;
using HatCommunityWebsite.Service.Dtos;
using HatCommunityWebsite.Service.Helpers;
using HatCommunityWebsite.Service.Responses;
using HatCommunityWebsite.Service.Responses.Data;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace HatCommunityWebsite.Service
{
    public interface ISubmissionService
    {
        Task Submit(SubmissionDto request, ClaimsIdentity userIdentity);

        Task RejectSubmission(RejectSubmissionDto request, ClaimsIdentity userIdentity);

        Task VerifySubmission(VerifySubmissionDto request, ClaimsIdentity userIdentity);

        Task DeleteSubmission(int runId, ClaimsIdentity userIdentity);

        Task UpdateSubmission(SubmissionDto request, ClaimsIdentity userIdentity);

        Task<SubmissionResponse> GetSubmission(int runId);

        Task ImportSubmissions(List<ImportDto> request, ClaimsIdentity userIdentity);
    }

    public class SubmissionService : ISubmissionService
    {
        private readonly IRunRepository _runRepo;
        private readonly ICategoryRepository _categoryRepo;
        private readonly IGameRepository _gameRepo;
        private readonly IUserRepository _userRepo;
        private readonly ISubcategoryRepository _subcategoryRepo;
        private readonly IVariableRepository _variableRepo;
        private readonly IVariableValueRepository _variableValueRepo;
        private readonly IRunVariableRepository _runVariableRepo;
        private readonly IMapper _mapper;

        public SubmissionService(
            IRunRepository runRepo,
            IMapper mapper,
            ICategoryRepository categoryRepository,
            ISubcategoryRepository subCategoryRepository,
            IGameRepository gameRepository,
            IUserRepository userRepository,
            IVariableRepository variableRepository,
            IRunVariableRepository runVariableRepository,
            IVariableValueRepository variableValueRepo)
        {
            _runRepo = runRepo;
            _categoryRepo = categoryRepository;
            _subcategoryRepo = subCategoryRepository;
            _userRepo = userRepository;
            _gameRepo = gameRepository;
            _mapper = mapper;
            _variableRepo = variableRepository;
            _runVariableRepo = runVariableRepository;
            _variableValueRepo = variableValueRepo;
        }

        public async Task Submit(SubmissionDto request, ClaimsIdentity userIdentity)
        {
            var run = new Run
            {
                Date = request.Date,
                Description = request.Description,
                Platform = request.Platform,
                Time = request.Time.Value,
                Videos = new List<Video>(),
                RunVariableValues = new List<RunVariableValue>(),
                RunUsers = new List<RunUser>()
            };

            var username = userIdentity.FindFirst(ClaimTypes.Name).Value;
            var userId = int.Parse(userIdentity.FindFirst("UserId").Value);
            var isAdmin = userIdentity.FindFirst(ClaimTypes.Role).Value == nameof(UserRoles.ROLE_ADMIN) ? true : false;

            await SetUsers(run, userId, request.ExtraUserIds);
            await SetCategories(request.CategoryId.Value, request.SubcategoryId, run);
            SetVideos(run, request.Videos);

            if (request.Variables != null)
                await SetRunVariables(request.Variables, run);

            run.Status = (int)SubmissionStatus.Pending;
            run.SubmittedDate = DateTime.UtcNow;
            run.SubmittedBy = username;

            if (request.AutoVerify && isAdmin)
            {
                await UpdateCurrentVerifiedRun(false, run, userId);
                run.Status = (int)SubmissionStatus.Verified;
                run.VerifiedDate = DateTime.UtcNow;
                run.VerifiedBy = username;
            }

            await _runRepo.SaveRun(run);

            await UpdatePendingRuns(userId, run.CategoryId.Value, run.SubcategoryId);
        }

        public async Task RejectSubmission(RejectSubmissionDto request, ClaimsIdentity userIdentity)
        {
            var run = await _runRepo.GetRunById(request.RunId);

            if (run == null)
                throw new AppException("Run not found");

            var modName = userIdentity.FindFirst(ClaimTypes.Name).Value;

            run.Status = (int)SubmissionStatus.Rejected;
            run.RejectedDate = DateTime.UtcNow;
            run.RejectedBy = modName;
            run.RejectedReason = request.RejectedReason;
            run.IsObsolete = true;

            run.VerifiedBy = null;
            run.VerifiedDate = null;

            //make last obsolete run visible
            await UpdateCurrentVerifiedRun(true, run, run.RunUsers.FirstOrDefault().UserId);

            await _runRepo.UpdateRun(run);

            await UpdatePendingRuns(run.RunUsers.FirstOrDefault().UserId, run.CategoryId.Value, run.SubcategoryId);
        }

        public async Task VerifySubmission(VerifySubmissionDto request, ClaimsIdentity userIdentity)
        {
            var run = await _runRepo.GetRunById(request.RunId);

            if (run == null)
                throw new AppException("Run not found");

            var modName = userIdentity.FindFirst(ClaimTypes.Name).Value;

            run.Status = (int)SubmissionStatus.Verified;
            run.VerifiedBy = modName;
            run.VerifiedDate = DateTime.UtcNow;

            //makes current board run obsolete
            await UpdateCurrentVerifiedRun(false, run, run.RunUsers.FirstOrDefault().UserId);

            await _runRepo.UpdateRun(run);

            await UpdatePendingRuns(run.RunUsers.FirstOrDefault().UserId, run.CategoryId.Value, run.SubcategoryId);
        }

        public async Task DeleteSubmission(int runId, ClaimsIdentity userIdentity)
        {
            var run = await _runRepo.GetRunById(runId);

            if (run == null)
                throw new AppException("Run not found");

            var username = userIdentity.FindFirst(ClaimTypes.Name).Value;

            if (run.SubmittedBy != username)
                throw new AppException("Action unauthorized");

            if (run.Status != (int)SubmissionStatus.Pending)
                throw new AppException("Only pending submissions can be deleted");

            await _runRepo.DeleteRun(run);

            await UpdatePendingRuns(run.RunUsers.FirstOrDefault().UserId, run.CategoryId.Value, run.SubcategoryId);
        }

        public async Task UpdateSubmission(SubmissionDto request, ClaimsIdentity userIdentity)
        {
            if (request.RunId == null)
                throw new AppException("Run id can't be null when updating");

            var run = await _runRepo.GetRunByIdWithRunVariablesAndVideos(request.RunId.Value);

            if (run == null)
                throw new AppException("Run not found");

            var username = userIdentity.FindFirst(ClaimTypes.Name).Value;
            var userId = int.Parse(userIdentity.FindFirst("UserId").Value);
            var isAdmin = userIdentity.FindFirst(ClaimTypes.Role).Value == nameof(UserRoles.ROLE_ADMIN) ? true : false;

            //only submission user or an admin can update the run
            if (run.SubmittedBy != username || !isAdmin)
                throw new AppException("Action unauthorized");

            if (request.SubcategoryId != null && request.SubcategoryId != run.SubcategoryId)
                run.SubCategory = await _subcategoryRepo.GetSubcategoryById(request.SubcategoryId.Value);

            run.Date = request.Date;
            run.Time = request.Time.Value;
            run.Platform = request.Platform;
            run.Description = request.Description;

            SetVideos(run, request.Videos);

            if (request.Variables != null)
                await UpdateRunVariable(request.Variables, run);

            //set run status to none

            if (run.Status != (int)SubmissionStatus.Pending)
            {
                run.VerifiedBy = null;
                run.VerifiedDate = null;
                run.RejectedBy = null;
                run.RejectedDate = null;
                run.RejectedReason = null;
            }

            //autoverification (admin only)
            if (request.AutoVerify && isAdmin)
            {
                if (run.Status == (int)SubmissionStatus.Verified) //if the run is verified just add verified name and date
                {
                    run.VerifiedDate = DateTime.UtcNow;
                    run.VerifiedBy = username;
                }
                else
                {
                    run.Status = (int)SubmissionStatus.Verified; //otherwise update the current user run on board
                    run.VerifiedDate = DateTime.UtcNow;
                    run.VerifiedBy = username;
                    await UpdateCurrentVerifiedRun(false, run, userId);
                }
            }
            else
            {
                if (run.Status != (int)SubmissionStatus.Pending) //set the status to pending and update the current user run on board
                {
                    await UpdateCurrentVerifiedRun(true, run, userId);
                    run.Status = (int)SubmissionStatus.Pending;
                }
            }

            await _runRepo.UpdateRun(run);

            //update all user pending runs
            await UpdatePendingRuns(userId, run.CategoryId.Value, run.SubcategoryId);
        }

        public async Task<SubmissionResponse> GetSubmission(int runId)
        {
            var run = await _runRepo.GetRunByIdIncludeAllData(runId);

            if (run == null)
                throw new AppException(string.Format("Run not found. Run id: {0}", runId));

            var response = await SetSubmissionData(run);

            return response;
        }

        public async Task ImportSubmissions(List<ImportDto> request, ClaimsIdentity userIdentity)
        {
            var runList = new List<Run>();

            foreach (var importedRun in request)
            {
                var run = new Run();
                run.RunUsers = new List<RunUser>();
                run.RunVariableValues = new List<RunVariableValue>();

                var users = await GetUser(importedRun.PlayerNames);

                foreach (var user in users)
                {
                    var runUser = new RunUser
                    {
                        AssociatedRun = run,
                        AssociatedUser = user,
                    };

                    run.RunUsers.Add(runUser);
                }

                var modUsername = userIdentity.FindFirst(ClaimTypes.Name).Value;

                run.Description = importedRun.Description;
                run.Platform = importedRun.Platform;
                run.Date = importedRun.Date;
                run.SubmittedBy = modUsername;
                run.IsObsolete = importedRun.IsObsolete;
                run.Status = importedRun.Status;
                run.SubmittedDate = DateTime.UtcNow;

                SetVideos(run, importedRun.Videos);
                await SetCategories(importedRun.CategoryId, importedRun.SubcategoryId, run);

                if (importedRun.Variables != null)
                    await SetRunVariables(importedRun.Variables, run);

                runList.Add(run);
            }

            await _runRepo.SaveRuns(runList);
        }

        //helper methods
        private async Task SetCategories(int categoryId, int? subCategoryId, Run run)
        {
            var category = await _categoryRepo.GetCategoryByIdIncludeSubcategories(categoryId);
            Subcategory? subcategory = null;

            if (category == null)
                throw new AppException("Category not found.");

            if (subCategoryId != null && !category.Subcategories.Any(x => x.Id == subCategoryId))
            {
                throw new AppException("Couldn't find subcategory.");
            }
            else
            {
                subcategory = category.Subcategories.FirstOrDefault(x => x.Id == subCategoryId);
            }

            run.Category = category;
            run.SubCategory = subcategory;
        }

        private void SetVideos(Run run, List<string> videosRequest)
        {
            run.Videos.Clear();

            foreach (var video in videosRequest)
                run.Videos.Add(new Video { Link = video, Run = run });
        }

        private async Task SetUsers(Run run, int userId, List<int>? userIds)
        {
            var submissionUser = await _userRepo.GetUserById(userId);

            run.RunUsers.Add(new RunUser
            {
                AssociatedUser = submissionUser,
                AssociatedRun = run,
            });

            if (userIds != null)
            {
                foreach (var id in userIds)
                {
                    var user = await _userRepo.GetUserById(id);

                    var runUser = new RunUser
                    {
                        AssociatedUser = user,
                        AssociatedRun = run,
                    };

                    run.RunUsers.Add(runUser);
                }
            }
        }

        private async Task<List<User>> GetUser(List<string> usernames)
        {
            var users = new List<User>();

            foreach (var username in usernames)
            {
                var user = await _userRepo.GetUserByUsername(username);

                if (user != null) //return the user if it exists
                {
                    users.Add(user);
                    continue;
                }

                CreateUser(username);

                var newUser = await _userRepo.GetUserByUsername(username);

                users.Add(newUser);
            }

            return users;
        }

        private void CreateUser(string username)
        {
            var user = new User();
            user.Role = (int)UserRoles.ROLE_BASIC;
            user.Created = DateTime.UtcNow;
            user.Email = "userwasimported";
            user.IsImported = true;

            // hash password
            CreatePasswordHash(Convert.ToHexString(RandomNumberGenerator.GetBytes(64)), out byte[] passwordHash, out byte[] passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            _userRepo.SaveUser(user);
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        private async Task<SubmissionResponse> SetSubmissionData(Run run)
        {
            var response = new SubmissionResponse();

            response.PlayerNames = GetPlayerNames(run.RunUsers);
            response.Platform = run.Platform;
            response.Description = run.Description;
            response.Time = run.Time;
            response.Videos = GetRunVideos(run.Videos);
            response.Date = run.Date;
            response.SubmittedBy = run.SubmittedBy;
            response.SubmittedDate = run.SubmittedDate;
            response.Place = await GetLeaderboardPlace(run);

            response.Status = run.Status;
            response.StatusLabel = Enum.GetName(typeof(SubmissionStatus), run.Status);
            response.VerifiedBy = run.VerifiedBy;
            response.VerifiedDate = run.VerifiedDate;
            response.RejectedBy = run.RejectedBy;
            response.RejectedDate = run.RejectedDate;
            response.RejectedReason = run.RejectedReason;
            response.IsObsolete = run.IsObsolete;

            response.Game = new GameData();
            response.Game.Id = run.Category.Game.Id;
            response.Game.Name = run.Category.Game.Name;
            response.Game.Acronym = run.Category.Game.Acronym;

            response.Category = new CategoryData();
            response.Category.Id = run.Category.Id;
            response.Category.Name = run.Category.Name;

            response.Subcategory = new SubcategoryData();
            response.Subcategory.Name = run.SubCategory?.Name;
            response.Subcategory.Id = run.SubCategory?.Id;

            response.Variables = new List<VariableData>();

            foreach (var var in run.RunVariableValues)
            {
                var variable = new VariableData
                {
                    VariableId = var.AssociatedVariableValue.VariableId,
                    VariableName = var.AssociatedVariableValue.Variable.Name,
                    Value = var.AssociatedVariableValue.Name,
                    ValueId = var.VariableValueId
                };
                response.Variables.Add(variable);
            }

            return response;
        }

        private List<string> GetRunVideos(List<Video> videos)
        {
            return videos.Select(x => x.Link).ToList();
        }

        private List<string> GetPlayerNames(ICollection<RunUser> runUsers)
        {
            return runUsers.Select(x => x.AssociatedUser.Username).ToList();
        }

        private async Task UpdatePendingRuns(int userId, int categoryId, int? subcategoryId)
        {
            //find user pending runs
            var pendingRuns = await _runRepo.GetUserPendingRuns(userId);

            if (pendingRuns == null)
                return;

            //find current verified run for the user
            var currentVerifiedRun = await _runRepo.GetCurrentVerifiedRun(userId, categoryId, subcategoryId);

            if (currentVerifiedRun == null)
                return;

            //update obsoletion accordingly
            foreach (var pendingRun in pendingRuns)
            {
                if (pendingRun.Time < currentVerifiedRun.Time)
                {
                    pendingRun.IsObsolete = false;
                    await _runRepo.UpdateRun(pendingRun);
                    continue;
                }

                pendingRun.IsObsolete = true;
                await _runRepo.UpdateRun(pendingRun);
            }
        }

        private async Task<string> GetLeaderboardPlace(Run run)
        {
            if (run.Status == (int)SubmissionStatus.Rejected) return string.Empty;

            if (run.IsObsolete) return "(Obsolete)";

            if (run.Status == (int)SubmissionStatus.Verified)
            {
                var runs = await _runRepo.GetLeaderboardRuns(run.CategoryId.Value, run.SubcategoryId);

                return (runs.FindIndex(x => x.Id == run.Id) + 1).ToString();
            }

            if (run.Status == (int)SubmissionStatus.Pending)
            {
                var runTimes = await _runRepo.GetLeaderboardTimes(run.CategoryId.Value, run.SubcategoryId);
                runTimes.Add(run.Time);

                var orderedRuns = runTimes.OrderBy(x => x).ToList();

                return (orderedRuns.FindIndex(x => x == run.Time) + 1).ToString();
            }

            return string.Empty;
        }

        private async Task SetRunVariables(List<VariableData> request, Run run)
        {
            foreach (var item in request)
            {
                var value = await _variableValueRepo.GetValueByNameAndVaribleId(item.VariableId, item.Value);

                if (value == null)
                    throw new AppException("Variable or value not found");

                var runvarRelationship = new RunVariableValue { AssociatedRun = run, AssociatedVariableValue = value };
                run.RunVariableValues.Add(runvarRelationship);
            }
        }

        private async Task UpdateRunVariable(List<VariableData> request, Run run)
        {
            foreach (var item in request)
            {
                var value = await _variableValueRepo.GetValueByNameAndVaribleId(item.VariableId, item.Value);

                if (value == null)
                    throw new AppException(string.Format("Value {0} not found. Id: {1}", item.Value, item.ValueId));

                if (run.RunVariableValues.FirstOrDefault(x => x.AssociatedVariableValue.VariableId == item.VariableId) == null)
                    throw new AppException(string.Format("Couldn't find association with variable name: {0}, id: {1}", item.VariableName, item.VariableId));

                run.RunVariableValues
                    .FirstOrDefault(x => x.AssociatedVariableValue.VariableId == item.VariableId).AssociatedVariableValue = value;
            }
        }

        private async Task UpdateCurrentVerifiedRun(bool isRejection, Run run, int userId) //make this spaghetti better
        {
            if (isRejection)
            {
                var lastVerifiedRun = await _runRepo.GetLastVerifiedRun(userId, run.CategoryId.Value, run.SubcategoryId);

                if (lastVerifiedRun != null)
                {
                    lastVerifiedRun.IsObsolete = false;
                    await _runRepo.UpdateRun(lastVerifiedRun);
                }
            }
            else
            {
                var currentVerifiedRun = await _runRepo.GetCurrentVerifiedRun(userId, run.CategoryId.Value, run.SubcategoryId);
                if (currentVerifiedRun != null)
                {
                    if (currentVerifiedRun.Time < run.Time)
                    {
                        run.IsObsolete = true;
                    }
                    else
                    {
                        run.IsObsolete = false;
                        currentVerifiedRun.IsObsolete = true;
                        await _runRepo.UpdateRun(currentVerifiedRun);
                    }
                }
                else
                {
                    run.IsObsolete = false;
                }
            }
        }
    }
}