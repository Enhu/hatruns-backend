using HatCommunityWebsite.DB;
using HatCommunityWebsite.Repo;
using HatCommunityWebsite.Service.Dtos;
using HatCommunityWebsite.Service.Helpers;
using HatCommunityWebsite.Service.Responses;

namespace HatCommunityWebsite.Service
{
    public interface IAdminService
    {
        Task<GameDashboardResponse> GetDashboardData(int gameId);

        Task UpdateGameInfo(UpdateGameInfoDto request);

        Task<string> HandleGameLevels(List<GameLevelDto> request);

        Task<string> HandleGameCategories(List<CategoryDto> request);

        Task<string> HandleGameVariables(List<VariableDto> request);
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

        public async Task<GameDashboardResponse> GetDashboardData(int gameId)
        {
            var game = await _gameRepo.GetGameByIdIncludeAll(gameId);
            var gameLevels = await _levelRepo.GetLevelsByGameIdIncludeAll(gameId);

            var request = new GameDashboardResponse();

            SetGameInfo(game, request);
            SetVariables(request, game.Variables);
            SetCategories(request, game.Categories);
            SetLevels(request, gameLevels);

            return request;
        }

        public async Task UpdateGameInfo(UpdateGameInfoDto request)
        {
            var game = await _gameRepo.GetGameById(request.Id.Value);

            if (game == null)
                throw new AppException("Game not found");

            game.Name = request.Name ?? game.Name;
            game.Acronym = request.Acronym ?? game.Acronym;
            game.ReleaseDate = request.ReleasedDate ?? game.ReleaseDate;
            game.IsActive = request.IsActive ?? game.IsActive;

            await _gameRepo.UpdateGame(game);
        }

        public async Task<string> HandleGameLevels(List<GameLevelDto> request)
        {
            var levelsToInsert = new List<Level>();
            var levelsToUpdate = new List<Level>();

            foreach (var levelRequest in request)
            {
                if (levelRequest.Id == null)
                {
                    var level = await AddNewLevel(levelRequest);
                    levelsToInsert.Add(level);
                }
            }

            if (levelsToInsert.Count > 0)
                await _levelRepo.SaveLevels(levelsToInsert);

            foreach (var levelRequest in request)
            {
                if (levelRequest.Id != null)
                {
                    var level = await UpdateLevel(levelRequest);
                    levelsToUpdate.Add(level);
                }
            }

            if (levelsToUpdate.Count > 0)
                await _levelRepo.UpdateLevels(levelsToUpdate);

            return string.Format("Task completed. Levels inserted: {0}. Levels updated: {1}", levelsToInsert.Count, levelsToUpdate.Count);
        }

        public async Task<string> HandleGameCategories(List<CategoryDto> request)
        {
            var categoriesToInsert = new List<Category>();
            var categoriesToUpdate = new List<Category>();

            foreach (var categoryRequest in request)
            {
                if (categoryRequest.Id == null)
                {
                    var category = AddNewCategory(categoryRequest);
                    categoriesToInsert.Add(category);
                }
            }

            if (categoriesToInsert.Count > 0)
                await _categoryRepo.SaveCategories(categoriesToInsert);

            foreach (var categoryRequest in request)
            {
                if (categoryRequest.Id != null)
                {
                    var category = await UpdateCategory(categoryRequest);
                    categoriesToUpdate.Add(category);
                }
            }

            if (categoriesToUpdate.Count > 0)
                await _categoryRepo.UpdateCategories(categoriesToUpdate);

            return string.Format("Task completed. Categories inserted: {0}. Categories updated: {1}", categoriesToInsert.Count, categoriesToUpdate.Count);
        }

        public async Task<string> HandleGameVariables(List<VariableDto> request)
        {
            var variablesToInsert = new List<Variable>();
            var variablesToUpdate = new List<Variable>();

            foreach (var variableRequest in request)
            {
                if (variableRequest.Id == null)
                {
                    var variable = await AddNewVariable(variableRequest);
                    variablesToInsert.Add(variable);
                }
            }

            if (variablesToInsert.Count > 0)
                await _variableRepo.SaveVariables(variablesToInsert);

            foreach (var variableRequest in request)
            {
                if (variableRequest.Id != null)
                {
                    var variable = await UpdateVariable(variableRequest);
                    variablesToUpdate.Add(variable);
                }
            }

            if (variablesToUpdate.Count > 0)
                await _variableRepo.UpdateVariables(variablesToUpdate);

            return string.Format("Task completed. Variables inserted: {0}. Variables updated: {1}", variablesToInsert.Count, variablesToUpdate.Count);
        }

        //helper methods

        //variable helper methods
        private async Task<Variable> AddNewVariable(VariableDto request)
        {
            var variable = new Variable();
            var values = new List<VariableValue>();

            if (string.IsNullOrEmpty(request.Name))
                throw new AppException("The field Name is required to create a new variable");

            var variableExists = await _variableRepo.VariableExistsByName(request.Name);

            if (variableExists)
                throw new AppException(string.Format("The name {0} already exists for a variable", request.Name));

            variable.Name = request.Name;
            variable.GameId = request.GameId.Value;

            foreach (var value in request.Values)
            {
                var newValue = new VariableValue
                {
                    Name = value.Name,
                    IsDefault = value.IsDefault.Value,
                };

                values.Add(newValue); 
            }

            variable.Values = values;

            return variable;
        }

        private async Task<Variable> UpdateVariable(VariableDto request)
        {
            var variable = await _variableRepo.GetVariableByIdIncludeValues(request.Id.Value);

            if (string.IsNullOrEmpty(request.Name))
                throw new AppException("The field Name is required to create a new variable");

            var variableExists = await _variableRepo.VariableExistsByName(request.Name);

            if (variableExists)
                throw new AppException(string.Format("The name {0} already exists for a variable", request.Name));

            variable.Name = request.Name ?? variable.Name;

            if (request.Values != null)
            {
                foreach (var value in request.Values)
                {
                    if (value.Id == null)
                        await AddNewValue(value, variable);
                    else
                        await UpdateValue(value, variable);
                }
            }

            return variable;
        }

        private async Task<VariableValue> UpdateValue(VariableValuesDto request, Variable variable)
        {
            var value = await _variableValueRepo.GetValueById(request.Id.Value);

            var valueExists = await _variableValueRepo.ValueExistsByNameAndVariableIdExcludeId(request.Name, variable.Id, value.Id);

            if (valueExists)
                throw new AppException(string.Format("The value {0} already exists for variable {1}", request.Name, variable.Name));

            value.Name = request.Name ?? value.Name;
            value.IsDefault = request.IsDefault ?? value.IsDefault;

            return value;
        }

        private async Task AddNewValue(VariableValuesDto request, Variable variable)
        {
            var value = new VariableValue();

            var valueExists = await _variableValueRepo.ValueExistsByNameAndVariableId(request.Name, variable.Id);

            if (valueExists)
                throw new AppException(string.Format("The value {0} already exists for variable {1}", request.Name, variable.Name));

            value.Name = request.Name;
            value.IsDefault = request.IsDefault.Value;
            variable.Values.Add(value);
        }

        //category helper methods
        private Category AddNewCategory(CategoryDto request)
        {
            if (string.IsNullOrEmpty(request.Name))
                throw new AppException("The field Name is required to create a new category");

            var category = new Category();

            category.Name = request.Name;
            category.Rules = request.Rules ?? category.Rules;
            category.Index = request.Index.Value;
            category.IsConsole = request.IsConsole ?? category.IsConsole;
            category.GameId = request.GameId.Value;

            if (request.Subcategories != null)
            {
                foreach (var subcategory in request.Subcategories)
                {
                    if (string.IsNullOrEmpty(subcategory.Name))
                        throw new AppException("The field Name is required to create a new subcategory");

                    AddNewSubcategory(subcategory, category);
                }
            }

            if (request.LevelId != null)
            {
                category.LevelId = request.LevelId.Value;
                category.IsCustom = request.IsCustom ?? category.IsCustom;
            }

            return category;
        }

        private async Task<Category> UpdateCategory(CategoryDto request)
        {
            var category = await _categoryRepo.GetCategoryByIdIncludeSubcategories(request.Id.Value);

            category.Name = request.Name ?? category.Name;
            category.Rules = request.Rules ?? category.Rules;
            category.Index = request.Index.Value;
            category.IsConsole = request.IsConsole ?? category.IsConsole;

            if (request.Subcategories != null)
            {
                foreach (var requestSubcategory in request.Subcategories)
                {
                    if (requestSubcategory.Id == null)
                    {
                        AddNewSubcategory(requestSubcategory, category);
                        continue;
                    }

                    var subcategory = category.Subcategories.Where(x => x.Id == requestSubcategory.Id.Value).FirstOrDefault();

                    if (subcategory == null)
                        throw new AppException(string.Format("Couldn't find subcategory for category {0} with subcategory Id: {1}", category.Name, requestSubcategory.Id.Value));

                    subcategory.Name = requestSubcategory.Name ?? subcategory.Name;
                    subcategory.Rules = requestSubcategory.Rules ?? subcategory.Rules;
                    subcategory.Index = requestSubcategory.Index.Value;
                }
            }

            return category;
        }

        private void AddNewSubcategory(SubcategoryDto requestSubcategory, Category category)
        {
            var subcategory = new Subcategory
            {
                Name = requestSubcategory.Name,
                Rules = requestSubcategory.Rules,
                Index = requestSubcategory.Index.Value
            };

            category.Subcategories.Add(subcategory);
        }

        //level helper methods
        private async Task<Level> AddNewLevel(GameLevelDto request)
        {
            var level = new Level();

            if (string.IsNullOrEmpty(request.Name))
                throw new AppException("The field Name is required to create a new game");

            var levelExists = await _levelRepo.LevelExistsByName(request.Name);

            if (levelExists)
                throw new AppException(string.Format("The name {0} already exists for a level", request.Name));

            level.GameId = request.GameId.Value;
            level.Rules = request.Rules ?? level.Rules;
            level.Name = request.Name;

            var categories = await _categoryRepo.GetAllLevelCategories();

            foreach (var levelCategory in categories)
            {
                var newCategory = new Category
                {
                    Name = levelCategory.Name,
                    IsConsole = levelCategory.IsConsole,
                    Rules = levelCategory.Rules,
                    Index = 0
                };

                level.Categories.Add(newCategory);
            }

            return level;
        }

        private async Task<Level> UpdateLevel(GameLevelDto request)
        {
            var level = await _levelRepo.GetLevelByIdIncludeCategories(request.Id.Value);

            if (level == null)
                throw new AppException("Level not found");

            var levelExists = await _levelRepo.LevelExistsByNameExluceId(request.Name, request.Id.Value);

            if (request.Name != null && levelExists)
                throw new AppException(string.Format("The name {0} already exists for a level", request.Name));

            level.Name = request.Name ?? level.Name;
            level.Rules = request.Rules ?? level.Rules;

            return level;
        }

        //data display helper methods

        private void SetLevels(GameDashboardResponse request, List<Level> gameLevels)
        {
            var levelData = new List<AdminLevelData>();

            foreach (var level in gameLevels)
            {
                var dashboardLevel = new AdminLevelData
                {
                    Id = level.Id,
                    Name = level.Name,
                };

                levelData.Add(dashboardLevel);
            }

            request.Levels = levelData;
        }

        private void SetCategories(GameDashboardResponse request, ICollection<Category> categories)
        {
            var categoryData = new List<AdminCategoryData>();

            foreach (var category in categories)
            {
                var dashboardCategory = new AdminCategoryData
                {
                    Id = category.Id,
                    Index = category.Index,
                    Name = category.Name,
                    Rules = category.Rules,
                    LevelId = category.LevelId,
                };

                SetSubcaetgories(category.Subcategories, dashboardCategory);

                categoryData.Add(dashboardCategory);
            }

            request.Categories = categoryData;
        }

        private static void SetSubcaetgories(ICollection<Subcategory> subcategories, AdminCategoryData dashboardCategory)
        {
            var subcategoryData = new List<AdminSubcategoryData>();

            foreach (var subcategory in subcategories)
            {
                var dashboardSubcategory = new AdminSubcategoryData
                {
                    Id = subcategory.Id,
                    Index = subcategory.Index,
                    Name = subcategory.Name,
                    Rules = subcategory.Rules,
                };

                subcategoryData.Add(dashboardSubcategory);
            }

            dashboardCategory.Subcategories = subcategoryData;
        }

        private void SetVariables(GameDashboardResponse request, ICollection<Variable> variables)
        {
            var variablesData = new List<AdminVariableData>();

            foreach (var variable in variables)
            {
                var adminVariable = new AdminVariableData
                {
                    Id = variable.Id,
                    Name = variable.Name,
                };

                SetValues(variable, adminVariable);

                variablesData.Add(adminVariable);
            }

            request.Variables = variablesData;
        }

        private static void SetValues(Variable variable, AdminVariableData adminVariable)
        {
            var valueData = new List<AdminValueData>();

            foreach (var value in variable.Values)
            {
                var varValue = new AdminValueData
                {
                    Value = value.Name,
                    IsDefault = value.IsDefault
                };

                valueData.Add(varValue);
            }

            adminVariable.Values = valueData;
        }

        private static void SetGameInfo(Game game, GameDashboardResponse request)
        {
            request.Id = game.Id;
            request.Name = game.Name;
            request.Acronym = game.Acronym;
            request.ReleaseDate = game.ReleaseDate;
            request.IsActive = game.IsActive;
        }
    }
}