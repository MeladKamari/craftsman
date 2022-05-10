﻿namespace Craftsman.Builders.Features;

using Domain;
using Domain.Enums;
using Helpers;
using Services;

public class CommandAddRecordBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public CommandAddRecordBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateCommand(string solutionDirectory, string srcDirectory, Entity entity, string contextName, string projectBaseName)
    {
        var classPath = ClassPathHelper.FeaturesClassPath(srcDirectory, $"{FileNames.AddEntityFeatureClassName(entity.Name)}.cs", entity.Plural, projectBaseName);
        var fileText = GetCommandFileText(classPath.ClassNamespace, entity, contextName, solutionDirectory, srcDirectory, projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetCommandFileText(string classNamespace, Entity entity, string contextName, string solutionDirectory, string srcDirectory, string projectBaseName)
    {
        var className = FileNames.AddEntityFeatureClassName(entity.Name);
        var addCommandName = FileNames.CommandAddName(entity.Name);
        var readDto = FileNames.GetDtoName(entity.Name, Dto.Read);
        var createDto = FileNames.GetDtoName(entity.Name, Dto.Creation);
        var manipulationValidator = FileNames.ValidatorNameGenerator(entity.Name, Validator.Manipulation);

        var entityName = entity.Name;
        var entityNameLowercase = entity.Name.LowercaseFirstLetter();
        var primaryKeyPropName = Entity.PrimaryKeyProperty.Name;
        var commandProp = $"{entityName}ToAdd";
        var newEntityProp = $"{entityNameLowercase}ToAdd";

        var entityClassPath = ClassPathHelper.EntityClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var dtoClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var exceptionsClassPath = ClassPathHelper.ExceptionsClassPath(srcDirectory, "");
        var contextClassPath = ClassPathHelper.DbContextClassPath(srcDirectory, "", projectBaseName);
        var validatorsClassPath = ClassPathHelper.ValidationClassPath(srcDirectory, "", entity.Plural, projectBaseName);

        return @$"namespace {classNamespace};

using {entityClassPath.ClassNamespace};
using {dtoClassPath.ClassNamespace};
using {exceptionsClassPath.ClassNamespace};
using {contextClassPath.ClassNamespace};
using {validatorsClassPath.ClassNamespace};
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public static class {className}
{{
    public class {addCommandName} : IRequest<{readDto}>
    {{
        public {createDto} {commandProp} {{ get; set; }}

        public {addCommandName}({createDto} {newEntityProp})
        {{
            {commandProp} = {newEntityProp};
        }}
    }}

    public class Handler : IRequestHandler<{addCommandName}, {readDto}>
    {{
        private readonly {contextName} _db;
        private readonly IMapper _mapper;

        public Handler({contextName} db, IMapper mapper)
        {{
            _mapper = mapper;
            _db = db;
        }}

        public async Task<{readDto}> Handle({addCommandName} request, CancellationToken cancellationToken)
        {{
            var {entityNameLowercase} = {entityName}.Create(request.{commandProp});
            _db.{entity.Plural}.Add({entityNameLowercase});

            await _db.SaveChangesAsync(cancellationToken);

            var {entityNameLowercase}Added = await _db.{entity.Plural}
                .AsNoTracking()
                .FirstOrDefaultAsync({entity.Lambda} => {entity.Lambda}.{primaryKeyPropName} == {entityNameLowercase}.{primaryKeyPropName}, cancellationToken);

            return _mapper.Map<{readDto}>({entityNameLowercase}Added);
        }}
    }}
}}";
    }
}
