using HatCommunityWebsite.DB;
using HatCommunityWebsite.Repo;
using HatCommunityWebsite.Service.Dtos;
using HatCommunityWebsite.Service.Responses;

namespace HatCommunityWebsite.Service
{
    public interface IAdminService
    {
        GameDashboardResponse GetDashboardData(int gameId);
        void UpdateGameInfo(UpdateGameInfoDto request);
        void HandleGameLevels(List<GameLevelDto> request);
        void HandleGameCategories(List<CategoryDto> request);
        void HandleGameVariables(List<VariableDto> request);

    }

    public class AdminService : IAdminService
    {
        private readonly IGameRepository _gameRepo;
        private readonly ILevelRepository _levelRepo;
        private readonly ICategoryRepository _categoryRepo;
        private readonly IVariableRepository _variableRepo;
        private readonly IVariableValueRepository _variableValueRepo;
        private readonly ISubcategoryRepository _subcategoryRepo;

        public AdminService(IGameRepository gameRepo, ILevelRepository levelRepo, ICategoryRepository categoryRepo,
            ISubcategoryRepository subcategoryRepo, IVariableRepository variableRepo, IVariableValueRepository variableValueRepo)
        {
            _gameRepo = gameRepo;
            _levelRepo = levelRepo;
            _categoryRepo = categoryRepo;
            _subcategoryRepo = subcategoryRepo;
            _variableRepo = variableRepo;
            _variableValueRepo = variableValueRepo;
        }

        public GameDashboardResponse GetDashboardData(int gameId)
        {
            var game = _gameRepo.GetGameByIdIncludeAll(gameId).Result;
            var gameLevels = _levelRepo.GetLevelsByGameIdIncludeAll(gameId).Result;

            var request = new GameDashboardResponse();

            SetVariables(request, game.Variables);
            SetCategories(request, game.Categories);
            SetLevels(request, gameLevels);

            return request;
        }

        public void UpdateGameInfo(UpdateGameInfoDto request)
        {
            var game = _gameRepo.GetGameById(request.Id).Result;

            game.Acronym = request.Acronym ?? game.Acronym;
            game.ReleaseDate = request.ReleasedDate ?? game.ReleaseDate;
            game.IsActive = request.IsActive ?? game.IsActive;

            _gameRepo.UpdateGame(game);
        }

        public void HandleGameLevels(List<GameLevelDto> request)
        {
            foreach (var level in request)
            {
                if (level.Id == null)
                    AddNewLevel(level);
                else
                    UpdateLevel(level);
            }
        }

        public void HandleGameCategories(List<CategoryDto> request)
        {
            foreach (var category in request)
            {
                if (category.Id == null)
                    AddNewCategory(category);
                else
                    UpdateCategory(category);
            }
        }

        public void HandleGameVariables(List<VariableDto> request)
        {
            foreach (var variable in request)
            {
                if (variable.Id == null)
                    AddNewVariable(variable);
                else
                    UpdateVariable(variable);
            }
        }

        //helper methods

        private void UpdateVariable(VariableDto request)
        {
            var variable = _variableRepo.GetVariableByIdIncludeValues(request.Id.Value).Result;

            variable.Name = request.Name ?? variable.Name;

            if (request.Values != null)
            {
                foreach (var value in request.Values)
                {
                    if (value.Id == null)
                        AddNewValue(value, variable);
                    else
                        UpdateValue(value);
                }
            }

            _variableRepo.UpdateVariable(variable);
        }

        private void UpdateValue(VariableValuesDto request)
        {
            var value = _variableValueRepo.GetValueById(request.Id.Value).Result;

            value.Name = request.Value ?? value.Name;
            value.IsDefault = request.IsDefault ?? value.IsDefault;

            _variableValueRepo.UpdateValue(value);
        }

        private void AddNewValue(VariableValuesDto request, Variable variable)
        {
            var value = new VariableValue();

            value.Name = request.Value;
            value.IsDefault = request.IsDefault.Value;
            variable.Values.Add(value);
        }

        private void AddNewVariable(VariableDto request)
        {
            var variable = new Variable();

            variable.Name = request.Name;

            foreach (var value in request.Values)
            {
                var newValue = new VariableValue
                {
                    Name = value.Value,
                    IsDefault = value.IsDefault.Value,
                };

                variable.Values.Add(newValue);
            }

            _variableRepo.SaveVariable(variable);
        }

        private void UpdateCategory(CategoryDto request)
        {
            var category = _categoryRepo.GetCategoryByIdIncludeSubcategories(request.Id.Value).Result;

            category.Name = request.Name ?? category.Name;
            category.Rules = request.Rules ?? category.Rules;
            category.IsDefault = request.IsDefault ?? category.IsDefault;
            category.IsConsole = request.IsConsole ?? category.IsConsole;

            if (request.Subcategories != null)
            {
                foreach (var requestSubcategory in request.Subcategories)
                {
                    if (requestSubcategory.Id == null)
                        AddNewSubcategory(requestSubcategory, category);

                    var subcategory = _subcategoryRepo.GetSubcategoryById(requestSubcategory.Id.Value).Result;

                    subcategory.Name = requestSubcategory.Name ?? subcategory.Name;
                    subcategory.Rules = requestSubcategory.Rules ?? subcategory.Rules;
                    subcategory.IsDefault = requestSubcategory.IsDefault ?? subcategory.IsDefault;

                    _subcategoryRepo.UpdateSubcategory(subcategory);
                }
            }

            _categoryRepo.UpdateCategory(category);
        }

        private void AddNewSubcategory(SubcategoryDto requestSubcategory, Category category)
        {
            var subcategory = new Subcategory
            {
                Name = requestSubcategory.Name,
                Rules = requestSubcategory.Rules,
                IsDefault = requestSubcategory.IsDefault.Value
            };

            category.Subcategories.Add(subcategory);
        }

        private void AddNewCategory(CategoryDto request)
        {
            var category = new Category();

            category.Name = request.Name ?? category.Name;
            category.Rules = request.Rules ?? category.Rules;
            category.IsDefault = request.IsDefault ?? category.IsDefault;
            category.IsConsole = request.IsConsole ?? category.IsConsole;

            if (request.Subcategories != null)
            {
                foreach (var subcategory in request.Subcategories)
                {
                    var newSubcategory = new Subcategory
                    {
                        Name = subcategory.Name,
                        IsDefault = subcategory.IsDefault.Value,
                        Rules = subcategory.Rules,
                    };

                    category.Subcategories.Add(newSubcategory);
                }
            }

            _categoryRepo.SaveCategory(category);
        }

        private void UpdateLevel(GameLevelDto request)
        {
            var level = _levelRepo.GetLevelByIdIncludeCategories(request.Id.Value).Result;

            level.Name = request.Name ?? level.Name;

            _levelRepo.UpdateLevel(level);
        }

        private void AddNewLevel(GameLevelDto request)
        {
            var level = new Level();

            level.Name = request.Name ?? level.Name;

            var categories = _categoryRepo.GetAllLevelCategories().Result;

            foreach (var levelCategory in categories)
            {
                var newCategory = new Category
                {
                    Name = levelCategory.Name,
                    IsConsole = levelCategory.IsConsole,
                    Rules = levelCategory.Rules,
                    IsDefault = levelCategory.IsDefault,
                };

                level.Categories.Add(newCategory);
            }

            _levelRepo.SaveLevel(level);
        }

        private void SetLevels(GameDashboardResponse request, List<Level> gameLevels)
        {
            foreach (var level in gameLevels)
            {
                var dashboardLevel = new AdminLevelData
                {
                    Id = level.Id,
                    Name = level.Name,
                };

                request.Levels.Add(dashboardLevel);
            }
        }

        private void SetCategories(GameDashboardResponse request, ICollection<Category> categories)
        {
            foreach (var category in categories)
            {
                var dashboardCategory = new AdminCategoryData
                {
                    Id = category.Id,
                    IsDefault = category.IsDefault,
                    Name = category.Name,
                    Rules = category.Rules,
                };

                SetSubcaetgories(category.Subcategories, dashboardCategory);

                request.Categories.Add(dashboardCategory);
            }
        }

        private static void SetSubcaetgories(ICollection<Subcategory> subcategories, AdminCategoryData dashboardCategory)
        {
            foreach (var subcategory in subcategories)
            {
                var dashboardSubcategory = new AdminSubcategoryData
                {
                    Id = subcategory.Id,
                    IsDefault = subcategory.IsDefault,
                    Name = subcategory.Name,
                    Rules = subcategory.Rules,
                };

                dashboardCategory.Subcategories.Add(dashboardSubcategory);
            }
        }

        private void SetVariables(GameDashboardResponse request, ICollection<Variable> variables)
        {
            foreach (var variable in variables)
            {
                var adminVariable = new AdminVariableData
                {
                    Id = variable.Id,
                    Name = variable.Name,
                };

                foreach (var value in variable.Values)
                {
                    var varValue = new AdminValueData
                    {
                        Value = value.Name,
                        IsDefault = value.IsDefault
                    };

                    adminVariable.Values.Add(varValue);
                }

                request.Variables.Add(adminVariable);
            }
        }
    }
}