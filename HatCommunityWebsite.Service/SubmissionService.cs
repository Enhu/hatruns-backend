using AutoMapper;
using HatCommunityWebsite.DB;
using HatCommunityWebsite.Repo;
using HatCommunityWebsite.Service.Dtos;
using HatCommunityWebsite.Service.Helpers;
using HatCommunityWebsite.Service.Responses;
using HatCommunityWebsite.Service.Responses.Data;
using Newtonsoft.Json;
using System.Security.Claims;

namespace HatCommunityWebsite.Service
{
    public interface ISubmissionService
    {
        void Submit(SubmissionDto request, ClaimsIdentity userIdentity);

        void RejectSubmission(RejectSubmissionDto request, ClaimsIdentity userIdentity);

        void VerifySubmission(VerifySubmissionDto request, ClaimsIdentity userIdentity);

        void DeleteSubmission(int runId, ClaimsIdentity userIdentity);

        void UpdateSubmission(UpdateSubmissionDto request, ClaimsIdentity userIdentity);

        SubmissionResponse GetSubmission(int runId);

        void ImportSubmissions(List<ImportDto> request, ClaimsIdentity userIdentity);
    }

    public class SubmissionService : ISubmissionService
    {
        private readonly IRunRepository _runRepo;
        private readonly ICategoryRepository _categoryRepo;
        private readonly IGameRepository _gameRepo;
        private readonly IUserRepository _userRepo;
        private readonly ISubCategoryRepository _subcategoryRepo;
        private readonly IVariableRepository _variableRepo;
        private readonly IRunVariableRepository _runVariableRepo;
        private readonly IMapper _mapper;

        public SubmissionService(
            IRunRepository runRepo,
            IMapper mapper,
            ICategoryRepository categoryRepository,
            ISubCategoryRepository subCategoryRepository,
            IGameRepository gameRepository,
            IUserRepository userRepository,
            IVariableRepository variableRepository,
            IRunVariableRepository runVariableRepository)
        {
            _runRepo = runRepo;
            _categoryRepo = categoryRepository;
            _subcategoryRepo = subCategoryRepository;
            _userRepo = userRepository;
            _gameRepo = gameRepository;
            _mapper = mapper;
            _variableRepo = variableRepository;
            _runVariableRepo = runVariableRepository;
        }

        public void Submit(SubmissionDto request, ClaimsIdentity userIdentity)
        {
            var run = _mapper.Map<Run>(request);

            var username = userIdentity.FindFirst(ClaimTypes.Name).Value;
            var userId = int.Parse(userIdentity.FindFirst("UserId").Value);
            var isAdmin = userIdentity.FindFirst(ClaimTypes.Role).Value == nameof(UserRoles.ROLE_ADMIN) ? true : false;

            SetUsers(run, userId, request.UserIds);
            SetVideos(run, request.Videos);
            SetCategories(request.CategoryId, request.SubcategoryId, run);

            if (!string.IsNullOrEmpty(request.Variables))
                SetRunVariables(request.Variables, run);

            run.Status = (int)SubmissionStatus.Pending;
            run.SubmittedDate = DateTime.UtcNow;
            run.SubmittedBy = username;

            if (request.AutoVerify && isAdmin)
            {
                UpdateCurrentVerifiedRun(false, run, userId);
                run.Status = (int)SubmissionStatus.Verified;
            }

            _runRepo.SaveRun(run);

            UpdatePendingRuns(userId, run.CategoryId, run.SubcategoryId);
        }

        public void RejectSubmission(RejectSubmissionDto request, ClaimsIdentity userIdentity)
        {
            var run = _runRepo.GetRunById(request.RunId).Result;

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
            UpdateCurrentVerifiedRun(true, run, run.RunUsers.FirstOrDefault().AssociatedUser.Id);

            _runRepo.UpdateRun(run);

            UpdatePendingRuns(run.RunUsers.FirstOrDefault().AssociatedUser.Id, run.CategoryId, run.SubcategoryId);
        }

        public void VerifySubmission(VerifySubmissionDto request, ClaimsIdentity userIdentity)
        {
            var run = _runRepo.GetRunById(request.RunId).Result;

            if (run == null)
                throw new AppException("Run not found");

            var modName = userIdentity.FindFirst(ClaimTypes.Name).Value;

            run.Status = (int)SubmissionStatus.Verified;
            run.VerifiedBy = modName;
            run.VerifiedDate = DateTime.UtcNow;

            //makes current board run obsolete
            UpdateCurrentVerifiedRun(false, run, run.RunUsers.FirstOrDefault().AssociatedUser.Id);

            _runRepo.UpdateRun(run);

            UpdatePendingRuns(run.RunUsers.FirstOrDefault().AssociatedUser.Id, run.CategoryId, run.SubcategoryId);
        }

        public void DeleteSubmission(int runId, ClaimsIdentity userIdentity)
        {
            var run = _runRepo.GetRunById(runId).Result;

            if (run == null)
                throw new AppException("Run not found");

            var username = userIdentity.FindFirst(ClaimTypes.Name).Value;

            if (run.SubmittedBy != username)
                throw new AppException("Action unauthorized");

            if (run.Status != (int)SubmissionStatus.Pending)
                throw new AppException("Only pending submissions can be deleted");

            _runRepo.DeleteRun(run);

            UpdatePendingRuns(run.RunUsers.FirstOrDefault().AssociatedUser.Id, run.CategoryId, run.SubcategoryId);
        }

        public void UpdateSubmission(UpdateSubmissionDto request, ClaimsIdentity userIdentity)
        {
            var run = _runRepo.GetRunByIdWithRunVariables(request.RunId).Result;

            if (run == null)
                throw new AppException("Run not found");

            var username = userIdentity.FindFirst(ClaimTypes.Name).Value;
            var userId = int.Parse(userIdentity.FindFirst("UserId").Value);
            var isAdmin = userIdentity.FindFirst(ClaimTypes.Role).Value == nameof(UserRoles.ROLE_ADMIN) ? true : false;

            if (run.SubmittedBy != username || !isAdmin)
                throw new AppException("Action unauthorized");

            if (request.SubcategoryId != run.SubcategoryId && request.SubcategoryId.HasValue)
                run.SubCategory = _subcategoryRepo.GetSubcategoryById((int)request.SubcategoryId).Result;

            run = _mapper.Map<Run>(request);

            run.Status = (int)SubmissionStatus.Pending;

            if (request.Variables != null) //makes this spaghetti better
            {
                var variables = JsonConvert.DeserializeObject<List<Variable>>(request.Variables);

                foreach (var item in variables) //THIS CAN BE IMPROVED
                {
                    var variable = _variableRepo.GetByNameAndId(item.Id, item.Name).Result;

                    var runVar = run.RunVariables.FirstOrDefault(x => x.VariableId == item.Id);
                    if (runVar != null)
                    {
                        run.RunVariables.Remove(runVar);
                    }

                    if (variable != null)
                    {
                        var runVariable = new RunVariable
                        {
                            AssociatedRun = run,
                            AssociatedVariable = variable
                        };

                        run.RunVariables.Add(runVariable);
                    }
                }
            }

            if (request.AutoVerify && isAdmin)
            {
                run.Status = (int)SubmissionStatus.Verified;
            }
            else
            {
                if (run.Status == (int)SubmissionStatus.Verified)
                {
                    UpdateCurrentVerifiedRun(true, run, userId);
                    run.Status = (int)SubmissionStatus.Pending;
                }
            }

            _runRepo.UpdateRun(run);

            UpdatePendingRuns(userId, run.CategoryId, run.SubcategoryId);
        }

        public SubmissionResponse GetSubmission(int runId)
        {
            var run = _runRepo.GetRunByIdWithAllRelationships(runId).Result;

            if (run == null)
                throw new AppException("Run not found");

            var response = _mapper.Map<SubmissionResponse>(run);
            SetSubmissionData(run, response);

            return response;
        }

        public void ImportSubmissions(List<ImportDto> request, ClaimsIdentity userIdentity) //not finished
        {
            var runList = new List<Run>();

            foreach (var importedRun in request)
            {
                var run = new Run();

                var users = GetUser(importedRun.PlayerNames); //finish

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
                run.Status = (int)SubmissionStatus.Pending;
                run.SubmittedDate = DateTime.UtcNow;

                SetVideos(run, importedRun.Videos);
                SetCategories(importedRun.CategoryId, importedRun.SubcategoryId, run);

                if (importedRun.Variables != null)
                    SetRunVariables(importedRun.Variables, run);

                runList.Add(run);
            }

            _runRepo.SaveRuns(runList);
        }

        //helper methods
        private void SetCategories(int categoryId, int? subCategoryId, Run run)
        {
            var category = _categoryRepo.GetCategoryById(categoryId).Result;
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

        private void SetVideos(Run run, List<string> videos)
        {
            foreach (var video in videos)
            {
                run.Videos.Add(new Video { Link = video, Run = run });
            }
        }

        private void SetUsers(Run run, int userId, List<int>? userIds)
        {
            var submissionUser = _userRepo.GetUserById(userId).Result;

            run.RunUsers.Add(new RunUser
            {
                AssociatedUser = submissionUser,
                AssociatedRun = run,
            });

            if (userIds != null)
            {
                foreach (var id in userIds)
                {
                    var user = _userRepo.GetUserById(id).Result;

                    var runUser = new RunUser
                    {
                        AssociatedUser = user,
                        AssociatedRun = run,
                    };

                    run.RunUsers.Add(runUser);
                }
            }
        }

        private List<User> GetUser(List<string> usernames)
        {
            var users = new List<User>();

            foreach (var username in usernames)
            {
                var user = _userRepo.GetUserByUsername(username).Result;

                if (user != null) //return the user if it exists
                {
                    users.Add(user);
                    continue;
                }

                CreateUser(username);

                var newUser = _userRepo.GetUserByUsername(username).Result;

                users.Add(newUser);
            }

            return users;
        }

        private void CreateUser(string username)
        {
            var user = new User
            {
                Username = username
            };

            _userRepo.SaveUser(user);
        }

        private void SetSubmissionData(Run run, SubmissionResponse response)
        {
            response.Game.Name = run.Category.Game.Name;
            response.Game.Acronym = run.Category.Game.Acronym;

            response.Category.Name = run.Category.Name;

            response.SubCategory.Name = run.SubCategory?.Name;

            foreach (var var in run.RunVariables)
            {
                response.Variables.Add(new VariablesData { Name = var.AssociatedVariable.Name, Value = var.AssociatedVariable.Value });
            }

            response.Place = GetLeaderboardPlace(run);
        }

        private void UpdatePendingRuns(int userId, int categoryId, int? subcategoryId)
        {
            //find user pending runs
            var pendingRuns = _runRepo.GetUserPendingRuns(userId).Result;

            if (pendingRuns == null)
                return;

            //find current verified run for the user
            var currentVerifiedRun = _runRepo.GetCurrentVerifiedRun(userId, categoryId, subcategoryId).Result;

            //update obsoletion accordingly
            foreach (var pendingRun in pendingRuns)
            {
                if (pendingRun.Time < currentVerifiedRun.Time)
                {
                    pendingRun.IsObsolete = false;
                    _runRepo.UpdateRun(pendingRun);
                    continue;
                }

                pendingRun.IsObsolete = true;
                _runRepo.UpdateRun(pendingRun);
            }
        }

        private string GetLeaderboardPlace(Run run)
        {
            if (run.Status == (int)SubmissionStatus.Rejected) return string.Empty;

            if (run.IsObsolete) return "(Obsolete)";

            if (run.Status == (int)SubmissionStatus.Verified)
            {
                var runs = _runRepo.GetLeaderboardRuns(run.CategoryId, run.SubcategoryId).Result;

                return (runs.FindIndex(x => x.Id == run.Id) + 1).ToString();
            }

            if (run.Status == (int)SubmissionStatus.Pending)
            {
                var runTimes = _runRepo.GetLeaderboardTimes(run.CategoryId, run.SubcategoryId).Result;
                runTimes.Add(run.Time);

                var orderedRuns = runTimes.OrderBy(x => x).ToList();

                return (orderedRuns.FindIndex(x => x == run.Time) + 1).ToString();
            }

            return string.Empty;
        }

        private void SetRunVariables(string request, Run run)
        {
            var variables = JsonConvert.DeserializeObject<List<Variable>>(request);

            foreach (var item in variables)
            {
                var variable = _variableRepo.GetByNameAndId(item.Id, item.Name).Result;

                if (variable == null)
                    throw new AppException("Variable not found");

                var runvarRelationship = new RunVariable { AssociatedRun = run, AssociatedVariable = variable };
                run.RunVariables.Add(runvarRelationship);
            }
        }

        private void UpdateCurrentVerifiedRun(bool isRejection, Run run, int userId) //make this spaghetti better
        {
            if (isRejection)
            {
                var lastVerifiedRun = _runRepo.GetLastVerifiedRun(userId, run.CategoryId, run.SubcategoryId).Result;

                if (lastVerifiedRun != null)
                {
                    lastVerifiedRun.IsObsolete = false;
                    _runRepo.UpdateRun(lastVerifiedRun);
                }
            }
            else
            {
                var currentVerifiedRun = _runRepo.GetCurrentVerifiedRun(userId, run.CategoryId, run.SubcategoryId).Result;
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
                        _runRepo.UpdateRun(currentVerifiedRun);
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