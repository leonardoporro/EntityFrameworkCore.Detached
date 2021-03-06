﻿using Detached.Mappers.Context;
using Detached.Mappers.TypeMaps;
using System.Linq.Expressions;
using static Detached.RuntimeTypes.Expressions.ExtendedExpression;
using static System.Linq.Expressions.Expression;

namespace Detached.Mappers.Factories.Entity
{
    public class EntityComposedMapperFactory : EntityMapperFactory
    {
        public override bool CanMap(TypeMap typeMap)
        {
            return typeMap.Parent != null
                && typeMap.IsComposition
                && typeMap.TargetOptions.IsEntity
                && typeMap.SourceOptions.IsComplexType;
        }

        public override LambdaExpression Create(TypeMap typeMap)
        {
            return Lambda(
                    GetDelegateType(typeMap),
                    Parameter(typeMap.Source),
                    Parameter(typeMap.Target),
                    Parameter(typeMap.Context),
                    Block(
                        CreateMemberMappers(typeMap),
                        CreateKey(typeMap),
                        If(IsNull(typeMap.Source),
                            Then(
                                If(IsNotNull(typeMap.Target),
                                    Then(
                                        OnMapperAction(typeMap, MapperActionType.Delete),
                                        Assign(typeMap.Target, Default(typeMap.Target.Type))
                                    )
                                )
                            ),
                            Else(
                                Variable("created", Constant(false), out Expression created),
                                If(IsNull(typeMap.Target),
                                    Then(
                                        Assign(typeMap.Target, typeMap.TargetOptions.Construct(typeMap.Context)),
                                        CreateMembers(typeMap, m => m.IsKey),
                                        Assign(created, Constant(true))
                                    ),
                                    Else(
                                        CreateKey(typeMap),
                                        If(NotEqual(typeMap.TargetKey, typeMap.SourceKey),
                                            Then(
                                                OnMapperAction(typeMap, MapperActionType.Delete),
                                                Assign(typeMap.Target, typeMap.TargetOptions.Construct(typeMap.Context)),
                                                CreateMembers(typeMap, m => m.IsKey),
                                                Assign(created, Constant(true))
                                            )
                                        )
                                    )
                                ),
                                CreateMembers(typeMap, m => !m.IsKey),
                                CreateBackReference(typeMap),
                                IfThenElse(created,
                                    OnMapperAction(typeMap, MapperActionType.Create),
                                    OnMapperAction(typeMap, MapperActionType.Update)
                                )
                            )
                        ),
                        Result(typeMap.Target)
                    )
                );
        }
    }
}