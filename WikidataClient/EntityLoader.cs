using EventSourcing.Services;
using EventSourcing.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Test.Helpers;
using WikidataClient.Model.Statement;
using WikidataClient.Model.Statement.Subjects;
using WikidataClient.Model.WikidataEntity;
using WikidataClient.Helpers;
using System.Text.RegularExpressions;
using WikidataClient.Model.Property;
using EventSourcing.Interfaces;

namespace WikidataEntityLoader
{
    public class EntityLoader
    {
        private const int _parameterLenghtLimit = 50;
        private readonly Regex _idPattern = new Regex("(^[Q,P,L]{1}[0-9]+$)|(^L[0-9]+-[F,S]{1}[0-9]+$)");

        private readonly EventService _eventService;
        private readonly HttpClient _httpClient;
        private List<JObject> _referencedEntities = new List<JObject>();
        private Dictionary<string, string> _redirects = new Dictionary<string, string>();

        public EntityLoader(EventService eventService, HttpClient httpClient)
        {
            _eventService = eventService;
            _httpClient = httpClient;
        }

        public async Task<WikidataEntity> LoadEntity(string id)
        {
            var jsonEntity = (await GetByIdsAsync(id)).Single();
            var type = jsonEntity.GetOrDefault("type", jsonEntity.Get<string>("id").Contains("S") ? "sense" : "form");

            var claims = jsonEntity.Get<JObject>("claims").ToObject<Dictionary<string, List<JObject>>>();

            _referencedEntities.Clear();
            _referencedEntities = await GetReferencedEntities(type, jsonEntity, claims);

            switch (type)
            {
                case "item":
                    #region Load WikidataItem
                    var wikidataItem = new WikidataItem();
                    var itemHandler = _eventService.GetHandler<WikidataItem, string>(wikidataItem);

                    itemHandler.Path(e => e.Id).Set(jsonEntity.Get<string>("id"));
                    itemHandler.Path(e => e.Type).Set(type);

                    await itemHandler.CreateAsync();

                    itemHandler.Path(e => e.PageId).Set(jsonEntity.GetOrDefault("pageid", -1));
                    itemHandler.Path(e => e.Ns).Set(jsonEntity.GetOrDefault("ns", -1));
                    itemHandler.Path(e => e.LastrevId).Set(jsonEntity.GetOrDefault("lastrevid", -1));
                    itemHandler.Path(e => e.Modified).Set(jsonEntity.GetOrDefault<DateTime>("modified"));
                    itemHandler.Path(e => e.Labels).Set(jsonEntity.GetToObject<Dictionary<string, JObject>>("labels").Select(pair => 
                            new Label
                            {
                                Id = pair.Key,
                                Language = pair.Key,
                                Value = pair.Value.Get<string>("value")
                            }).ToList());

                    itemHandler.Path(e => e.Descriptions).Set(jsonEntity.GetToObject<Dictionary<string, JObject>>("descriptions").Select(pair => 
                            new Description
                            {
                                Id = pair.Key,
                                Language = pair.Key,
                                Value = pair.Value.Get<string>("value")
                            }).ToList());

                    itemHandler.Path(e => e.Aliases).Set(jsonEntity.GetToObject<Dictionary<string, List<JObject>>>("aliases").Select(pair => 
                            new Alias
                            {
                                Id = pair.Key,
                                Language = pair.Key,
                                Values = pair.Value.Select(v => v.Get<string>("value")).ToList()
                            }).ToList());

                    itemHandler.Path(e => e.SiteLinks).Set(jsonEntity.GetToObject<Dictionary<string, JObject>>("sitelinks").Select(pair => 
                            new SiteLink
                            {
                                Id = pair.Key,
                                Site = pair.Key,
                                Title = pair.Value.Get<string>("title"),
                                Badges = pair.Value.Get<JArray>("badges").ToObject<List<string>>()
                            }).ToList());

                    itemHandler.Path(e => e.Statements).Set(GetStatements(claims, _referencedEntities));

                    if (jsonEntity.GetOrDefault<JObject>("redirects") is JObject itemRedirects)
                    {
                        itemHandler.Path(e => e.Redirects).Set(new Redirects
                        {
                            From = itemRedirects.Get<string>("from"),
                            To = itemRedirects.Get<string>("to")
                        });
                    }

                    await itemHandler.UpdateAsync();

                    return wikidataItem;
                #endregion

                case "property":
                    #region Load WikidataProperty
                    var wikidataProperty = new WikidataProperty();
                    var propertyHandler = _eventService.GetHandler<WikidataProperty, string>(wikidataProperty);

                    propertyHandler.Path(e => e.Id).Set(jsonEntity.Get<string>("id"));
                    propertyHandler.Path(e => e.Type).Set(type);

                    await propertyHandler.CreateAsync();

                    propertyHandler.Path(e => e.DataType).Set(jsonEntity.GetOrDefault("datatype", string.Empty));
                    propertyHandler.Path(e => e.PageId).Set(jsonEntity.GetOrDefault("pageid", -1));
                    propertyHandler.Path(e => e.Ns).Set(jsonEntity.GetOrDefault("ns", -1));
                    propertyHandler.Path(e => e.LastrevId).Set(jsonEntity.GetOrDefault("lastrevid", -1));
                    propertyHandler.Path(e => e.Modified).Set(jsonEntity.GetOrDefault<DateTime>("modified"));

                    propertyHandler.Path(e => e.Labels).Set(jsonEntity.GetToObject<Dictionary<string, JObject>>("labels").Select(pair => 
                            new Label
                            {
                                Id = pair.Key,
                                Language = pair.Key,
                                Value = pair.Value.Get<string>("value")
                            }).ToList());

                    propertyHandler.Path(e => e.Descriptions).Set(jsonEntity.GetToObject<Dictionary<string, JObject>>("descriptions").Select(pair => 
                            new Description
                            {
                                Id = pair.Key,
                                Language = pair.Key,
                                Value = pair.Value.Get<string>("value")
                            }).ToList());

                    propertyHandler.Path(e => e.Aliases).Set(jsonEntity.GetToObject<Dictionary<string, List<JObject>>>("aliases").Select(pair => 
                            new Alias
                            {
                                Id = pair.Key,
                                Language = pair.Key,
                                Values = pair.Value.Select(v => v.Get<string>("value")).ToList()
                            }).ToList());

                    propertyHandler.Path(e => e.Statements).Set(GetStatements(claims, _referencedEntities));

                    if (jsonEntity.GetOrDefault<JObject>("redirects") is JObject propertyRedirects)
                    {
                        propertyHandler.Path(e => e.Redirects).Set(new Redirects
                        {
                            From = propertyRedirects.Get<string>("from"),
                            To = propertyRedirects.Get<string>("to")
                        });
                    }

                    await propertyHandler.UpdateAsync();

                    return wikidataProperty;
                #endregion

                case "lexeme":
                    #region Load WikidataLexeme
                    var wikidataLexeme = new WikidataLexeme();
                    var lexemeHandler = _eventService.GetHandler<WikidataLexeme, string>(wikidataLexeme);

                    lexemeHandler.Path(e => e.Id).Set(jsonEntity.Get<string>("id"));
                    lexemeHandler.Path(e => e.Type).Set(type);

                    await lexemeHandler.CreateAsync();

                    lexemeHandler.Path(e => e.PageId).Set(jsonEntity.GetOrDefault("pageid", -1));
                    lexemeHandler.Path(e => e.Ns).Set(jsonEntity.GetOrDefault("ns", -1));
                    lexemeHandler.Path(e => e.LastrevId).Set(jsonEntity.GetOrDefault("lastrevid", -1));
                    lexemeHandler.Path(e => e.Modified).Set(jsonEntity.GetOrDefault<DateTime>("modified"));

                    lexemeHandler.Path(e => e.Statements).Set(GetStatements(claims, _referencedEntities));

                    lexemeHandler.Path(e => e.Lemmas).Set(jsonEntity.GetToObject<Dictionary<string, JObject>>("lemmas").Select(pair => 
                            new Lemma
                            {
                                Id = pair.Key,
                                Language = pair.Key,
                                Value = pair.Value.Get<string>("value")
                            }).ToList());

                    lexemeHandler.Path(e => e.Forms).Set(jsonEntity.Get<JArray>("forms").Select(form => {
                                var representation = form.GetOrFirst("representations", "en");
                                var language = representation.GetOrDefault("language", string.Empty);
                                return new Form
                                {
                                    Id = form.GetOrDefault("id", string.Empty),
                                    Representations = new()
                                    {
                                        new Representation
                                        {
                                            Id = language,
                                            Value = representation.GetOrDefault("value", string.Empty),
                                            Language = language
                                        }
                                    }
                                };
                            }).ToList());

                    lexemeHandler.Path(e => e.Senses).Set(jsonEntity.Get<JArray>("senses").Select(sense => {
                            var gloss = sense.GetOrFirst("glosses", "en");
                            var language = gloss.GetOrDefault("language", string.Empty);
                            return new Sense
                                {
                                    Id = sense.GetOrDefault("id", string.Empty),
                                    Glosses = new()
                                    {
                                        new Gloss
                                        {
                                            Id = language,
                                            Value = gloss.GetOrDefault("value", string.Empty),
                                            Language = language
                                        }
                                    }
                                };
                            }).ToList());

                    if (jsonEntity.GetOrDefault<string>("lexicalCategory") is string lexicalCategoryId)
                    {
                        var jsonLexicalCategory = (await GetByIdsAsync(lexicalCategoryId)).Single();
                        var label = jsonLexicalCategory.GetOrFirst("labels", "en");
                        var labelLanguage = label.GetOrDefault("language", string.Empty);
                        var description = jsonLexicalCategory.GetOrFirst("descriptions", "en");
                        var descriptionLanguage = description.GetOrDefault("language", string.Empty);
                        lexemeHandler.Path(e => e.LexicalCategory).Set(new Item
                        {
                            Id = lexicalCategoryId,
                            Labels = new()
                            {
                                new Label
                                {
                                    Id = labelLanguage,
                                    Value = label.GetOrDefault("value", string.Empty),
                                    Language = labelLanguage
                                }
                            },
                            Descriptions = new()
                            {
                                new Description
                                {
                                    Id = descriptionLanguage,
                                    Value = description.GetOrDefault("value", string.Empty),
                                    Language = descriptionLanguage
                                }
                            }
                        });
                    }

                    if (jsonEntity.GetOrDefault<string>("language") is string languageId)
                    {
                        var jsonLanguage = (await GetByIdsAsync(languageId)).Single();
                        var label = jsonLanguage.GetOrFirst("labels", "en");
                        var labelLanguage = label.GetOrDefault("language", string.Empty);
                        var description = jsonLanguage.GetOrFirst("descriptions", "en");
                        var descriptionLanguage = description.GetOrDefault("language", string.Empty);
                        lexemeHandler.Path(e => e.Language).Set(new Item
                        {
                            Id = languageId,
                            Labels = new()
                            {
                                new Label
                                {
                                    Id = labelLanguage,
                                    Value = label.GetOrDefault("value", string.Empty),
                                    Language = labelLanguage
                                }
                            },
                            Descriptions = new()
                            {
                                new Description
                                {
                                    Id = descriptionLanguage,
                                    Value = description.GetOrDefault("value", string.Empty),
                                    Language = descriptionLanguage
                                }
                            }
                        });
                    }

                    if (jsonEntity.GetOrDefault<JObject>("redirects") is JObject lexemeRedirects)
                    {
                        lexemeHandler.Path(e => e.Redirects).Set(new Redirects
                        {
                            From = lexemeRedirects.Get<string>("from"),
                            To = lexemeRedirects.Get<string>("to")
                        });
                    }

                    await lexemeHandler.UpdateAsync();

                    return wikidataLexeme;
                    #endregion

                case "form":
                    #region Load WikidataForm
                    var wikidataForm = new WikidataForm();
                    var formHandler = _eventService.GetHandler<WikidataForm, string>(wikidataForm);

                    formHandler.Path(e => e.Id).Set(jsonEntity.Get<string>("id"));
                    formHandler.Path(e => e.Type).Set(type);

                    await formHandler.CreateAsync();

                    formHandler.Path(e => e.PageId).Set(jsonEntity.GetOrDefault("pageid" , -1));
                    formHandler.Path(e => e.Ns).Set(jsonEntity.GetOrDefault("ns", -1));
                    formHandler.Path(e => e.LastrevId).Set(jsonEntity.GetOrDefault("lastrevid", -1));
                    formHandler.Path(e => e.Modified).Set(jsonEntity.GetOrDefault<DateTime>("modified"));

                    formHandler.Path(e => e.Statements).Set(GetStatements(claims, _referencedEntities));

                    formHandler.Path(e => e.Representations).Set(jsonEntity.GetToObject<Dictionary<string, JObject>>("representations").Select(pair => 
                            new Representation
                            {
                                Id = pair.Key,
                                Language = pair.Key,
                                Value = pair.Value.Get<string>("value")
                            }).ToList());

                    formHandler.Path(e => e.GrammaticalFeatures).Set(jsonEntity.Get<JArray>("grammaticalFeatures").Select(async grammaticalFeature =>
                        {
                            var grammaticalFeatureId = grammaticalFeature.Value<string>();
                            var jsonGrammaticalFeatures = (await GetByIdsAsync(grammaticalFeatureId)).Single();
                            var label = jsonGrammaticalFeatures.GetOrFirst("labels", "en");
                            var labelLanguage = label.GetOrDefault("language", string.Empty);
                            var description = jsonGrammaticalFeatures.GetOrFirst("descriptions", "en");
                            var descriptionLanguage = description.GetOrDefault("language", string.Empty);
                            return new Item
                            {
                                Id = grammaticalFeatureId,
                                Labels = new()
                                {
                                    new Label
                                    {
                                        Id = labelLanguage,
                                        Value = label.GetOrDefault("value", string.Empty),
                                        Language = labelLanguage
                                    }
                                },
                                Descriptions = new()
                                {
                                    new Description
                                    {
                                        Id = descriptionLanguage,
                                        Value = description.GetOrDefault("value", string.Empty),
                                        Language = descriptionLanguage
                                    }
                                }
                            };
                        }).Select(t => t.Result).ToList());

                    if (jsonEntity.GetOrDefault<JObject>("redirects") is JObject formRedirects)
                    {
                        formHandler.Path(e => e.Redirects).Set(new Redirects
                        {
                            From = formRedirects.Get<string>("from"),
                            To = formRedirects.Get<string>("to")
                        });
                    }

                    await formHandler.UpdateAsync();

                    return wikidataForm;
                #endregion

                case "sense":
                    #region Load WikidataSense
                    var wikidataSense = new WikidataSense();
                    var senseHandler = _eventService.GetHandler<WikidataSense, string>(wikidataSense);

                    senseHandler.Path(e => e.Id).Set(jsonEntity.Get<string>("id"));
                    senseHandler.Path(e => e.Type).Set(type);

                    await senseHandler.CreateAsync();

                    senseHandler.Path(e => e.PageId).Set(jsonEntity.GetOrDefault("pageid", -1));
                    senseHandler.Path(e => e.Ns).Set(jsonEntity.GetOrDefault("ns", -1));
                    senseHandler.Path(e => e.LastrevId).Set(jsonEntity.GetOrDefault("lastrevid", -1));
                    senseHandler.Path(e => e.Modified).Set(jsonEntity.GetOrDefault<DateTime>("modified"));

                    senseHandler.Path(e => e.Statements).Set(GetStatements(claims, _referencedEntities));

                    senseHandler.Path(e => e.Glosses).Set(jsonEntity.GetToObject<Dictionary<string, JObject>>("glosses").Select(pair => 
                            new Gloss
                            {
                                Id = pair.Key,
                                Language = pair.Key,
                                Value = pair.Value.Get<string>("value")
                            }).ToList());

                    if (jsonEntity.GetOrDefault<JObject>("redirects") is JObject senseRedirects)
                    {
                        senseHandler.Path(e => e.Redirects).Set(new Redirects
                        {
                            From = senseRedirects.Get<string>("from"),
                            To = senseRedirects.Get<string>("to")
                        });
                    }

                    //await senseHandler.UpdateAsync();

                    return wikidataSense;
                #endregion

                default:
                    throw new Exception($"Invalid wikidata entity type: {type}");
            }
        }

        private List<Statement> GetStatements(Dictionary<string, List<JObject>> claims, List<JObject> referencedEntities) 
        {
            var statements = new List<Statement>();

            foreach (var pair in claims)
            {
                var predicateEntity = GetReferencedEntityById(pair.Key);
                var predicateLabel = predicateEntity.GetOrFirst("labels", "en");
                var predicateLabelLanguage = predicateLabel.GetOrDefault("language", string.Empty);
                var predicate = new Predicate
                {
                    Id = pair.Key,
                    Labels = new()
                    {
                        new Label
                        {
                            Id = predicateLabelLanguage,
                            Value = predicateLabel.GetOrDefault("value", string.Empty),
                            Language = predicateLabelLanguage
                        }
                    }
                };

                var subjects = new List<Subject>();

                foreach (var sub in pair.Value)
                {
                    var mainsnak = sub.Get<JObject>("mainsnak"); 
                    var rank = sub.Get<string>("rank");
                    if (mainsnak.GetOrDefault<JObject>("datavalue") is JObject dataValue) 
                    {
                        var references = new List<Statement>();

                        if (sub.GetOrDefault<JArray>("references") is JArray refs)
                        {
                            foreach (var refeGroup in refs)
                            {
                                foreach (var snak in refeGroup.GetToObject<Dictionary<string, JArray>>("snaks"))
                                {
                                    var refPredicateEntity = GetReferencedEntityById(snak.Key);
                                    var refPredicateEntityLabel = refPredicateEntity.GetOrFirst("labels", "en");
                                    var refPredicateEntityLabelLanguage = refPredicateEntityLabel.GetOrDefault("language", string.Empty);
                                    var refPredicate = new Predicate
                                    {
                                        Id = snak.Key,
                                        Labels = new()
                                        {
                                            new Label
                                            {
                                                Id = refPredicateEntityLabelLanguage,
                                                Value = refPredicateEntityLabel.GetOrDefault("value", string.Empty),
                                                Language = refPredicateEntityLabelLanguage
                                            }
                                        }
                                    };

                                    var refSubjects = new List<Subject>();
                                    foreach (var reference in snak.Value)
                                    {
                                        if (reference.GetOrDefault<JObject>("datavalue") is JObject referenceDataValue)
                                        {
                                            refSubjects.Add(GetSpecificSubject(referenceDataValue, reference.Get<string>("datatype")));
                                        }
                                    }

                                    references.Add(new Statement
                                    {
                                        Id = refPredicate.Id,
                                        Predicate = refPredicate,
                                        Subjects = refSubjects
                                    });
                                }
                            }
                        }

                        var qualifiers = new List<Statement>();

                        if (sub.ContainsKey("qualifiers"))
                        {
                            var quals = sub.GetToObject<Dictionary<string, List<JObject>>>("qualifiers");

                            foreach (var qualifier in quals)
                            {
                                var qualPredicateEntity = GetReferencedEntityById(qualifier.Key);
                                var qualPredicateEntityLabel = qualPredicateEntity.GetOrFirst("labels", "en");
                                var qualPredicateEntityLabelLanguage = qualPredicateEntityLabel.GetOrDefault("language", string.Empty);
                                var qualPredicate = new Predicate
                                {
                                    Id = qualifier.Key,
                                    Labels = new()
                                    {
                                        new Label
                                        {
                                            Id = qualPredicateEntityLabelLanguage,
                                            Value = qualPredicateEntityLabel.GetOrDefault("value", string.Empty),
                                            Language = qualPredicateEntityLabelLanguage
                                        }
                                    }
                                };

                                var qualSubjects = new List<Subject>();
                                foreach (var qual in qualifier.Value)
                                {
                                    if (qual.GetOrDefault<JObject>("datavalue") is JObject qualifierDataValue)
                                    {
                                        qualSubjects.Add(GetSpecificSubject(qualifierDataValue, qual.Get<string>("datatype")));
                                    }
                                }

                                qualifiers.Add(new Statement
                                {
                                    Id = qualPredicate.Id,
                                    Predicate = qualPredicate,
                                    Subjects = qualSubjects
                                });
                            }
                        }

                        var specificSubject = GetSpecificSubject(dataValue, mainsnak.Get<string>("datatype"), rank);
                        specificSubject.References = references;
                        specificSubject.Qualifiers = qualifiers;

                        subjects.Add(specificSubject);
                    }
                }

                statements.Add(new Statement
                {
                    Id = predicate.Id,
                    Predicate = predicate,
                    Subjects = subjects
                });
            }

            return statements;
        }

        private Subject GetSpecificSubject(JObject dataValue, string datatype, string rank = "")
        {
            switch (datatype)
            {
                case CommonsMedia.dataType:
                    return new CommonsMedia
                    {
                        Value = dataValue.Get<string>("value"),
                        Rank = rank
                    };
                case ExternalIdentifier.dataType:
                    return new ExternalIdentifier
                    {
                        Value = dataValue.Get<string>("value"),
                        Rank = rank
                    };
                case Form.dataType:
                    var formValue = dataValue.Get<JObject>("value");
                    var formId = formValue.Get<string>("id");
                    var form = GetReferencedEntityById(formId);
                    var representation = form.GetOrFirst("representations", "en");
                    var representationLanguage = representation.GetOrDefault("language", string.Empty);
                    return new Form
                    {
                        Id = formId,
                        Representations = new()
                        {
                            new Representation
                            {
                                Id = representationLanguage,
                                Value = representation.GetOrDefault("value", string.Empty),
                                Language = representationLanguage
                            }
                        },
                        EntityType = formValue.Get<string>("entity-type"),
                        Rank = rank
                    };
                case GeographicShape.dataType:
                    return new GeographicShape
                    {
                        Value = dataValue.Get<string>("value"),
                        Rank = rank
                    };
                case GlobeCoordinate.dataType:
                    var globecoordinateValue = dataValue.Get<JObject>("value");
                    return new GlobeCoordinate
                    {
                        Latitude = globecoordinateValue.Get<float>("latitude"),
                        Longitude = globecoordinateValue.Get<float>("longitude"),
                        Altitude = globecoordinateValue.Get<object>("altitude"),
                        Precision = globecoordinateValue.Get<float>("precision"),
                        Globe = globecoordinateValue.Get<string>("globe"),
                        Rank = rank
                    };
                case Item.dataType:
                    var itemValue = dataValue.Get<JObject>("value");
                    var itemId = itemValue.Get<string>("id");
                    var item = GetReferencedEntityById(itemId);
                    var itemLabel = item.GetOrFirst("labels", "en");
                    var itemLabelLanguage = itemLabel.GetOrDefault("language", string.Empty);
                    var itemDescription = item.GetOrFirst("descriptions", "en");
                    var itemDescriptionLanguage = itemDescription.GetOrDefault("language", string.Empty);
                    return new Item
                    {
                        Id = itemId,
                        Labels = new()
                        {
                            new Label
                            {
                                Id = itemLabelLanguage,
                                Value = itemLabel.GetOrDefault("value", string.Empty),
                                Language = itemLabelLanguage
                            }
                        },
                        Descriptions = new()
                        {
                            new Description
                            {
                                Id = itemDescriptionLanguage,
                                Value = itemDescription.GetOrDefault("value", string.Empty),
                                Language = itemDescriptionLanguage
                            }
                        },
                        NumericId = itemValue.Get<int>("numeric-id"),
                        EntityType = itemValue.Get<string>("entity-type"),
                        Rank = rank
                    };
                case Lexeme.dataType:
                    var lexemeValue = dataValue.Get<JObject>("value");
                    var lexemeId = lexemeValue.Get<string>("id");
                    var lexeme = GetReferencedEntityById(lexemeId);
                    var lexemeLemma = lexeme.GetOrFirst("lemmas", "en");
                    var lexemeLemmaLanguage = lexemeLemma.GetOrDefault("language", string.Empty);
                    return new Lexeme
                    {
                        Id = lexemeId,
                        Lemmas = new()
                        {
                            new Lemma
                            {
                                Id = lexemeLemmaLanguage,
                                Value = lexemeLemma.GetOrDefault("value", string.Empty),
                                Language = lexemeLemmaLanguage
                            }
                        },
                        NumericId = lexemeValue.Get<int>("numeric-id"),
                        EntityType = lexemeValue.Get<string>("entity-type"),
                        Rank = rank
                    };
                case MathematicalExpression.dataType:
                    return new MathematicalExpression
                    {
                        Value = dataValue.Get<string>("value"),
                        Rank = rank
                    };
                case MonolingualText.dataType:
                    var monolingualtextValue = dataValue.Get<JObject>("value");
                    return new MonolingualText
                    {
                        Text = monolingualtextValue.Get<string>("text"),
                        Language = monolingualtextValue.GetOrDefault("language", string.Empty),
                        Rank = rank
                    };
                case MusicalNotation.dataType:
                    return new MusicalNotation
                    {
                        Value = dataValue.Get<string>("value"),
                        Rank = rank
                    };
                case Property.dataType:
                    var propertyValue = dataValue.Get<JObject>("value");
                    var propertyId = propertyValue.Get<string>("id");
                    var property = GetReferencedEntityById(propertyId);
                    var propertyLabel = property.GetOrFirst("labels", "en");
                    var propertyLabelLanguage = propertyLabel.GetOrDefault("language", string.Empty);
                    var propertyDescription = property.GetOrFirst("descriptions", "en");
                    var propertyDescriptionLanguage = propertyDescription.GetOrDefault("language", string.Empty);
                    return new Property
                    {
                        Id = propertyId,
                        Labels = new()
                        {
                            new Label
                            {
                                Id = propertyLabelLanguage,
                                Value = propertyLabel.GetOrDefault("value", string.Empty),
                                Language = propertyLabelLanguage
                            }
                        },
                        Descriptions = new()
                        {
                            new Description
                            {
                                Id = propertyDescriptionLanguage,
                                Value = propertyDescription.GetOrDefault("value", string.Empty),
                                Language = propertyDescriptionLanguage
                            }
                        },
                        NumericId = propertyValue.Get<int>("numeric-id"),
                        EntityType = propertyValue.Get<string>("entity-type"),
                        Rank = rank
                    };
                case Quantity.dataType:
                    var quantityValue = dataValue.Get<JObject>("value");
                    var unitId = quantityValue.Get<string>("unit");
                    JObject quantityLabel = null;
                    string quantityLabelLanguage = null;
                    if (SetIdIfValid(ref unitId))
                    {
                        unitId = $"Q{unitId.Split('Q')[1]}";
                        var unitEntity = GetReferencedEntityById(unitId);
                        quantityLabel = unitEntity.GetOrFirst("labels", "en");
                        quantityLabelLanguage = quantityLabel.GetOrDefault("language", string.Empty);
                    }
                    else
                    {
                        unitId = Guid.NewGuid().ToString();
                    }
                    return new Quantity
                    {
                        Label = quantityLabel is not null ? new Label
                        {
                            Id = quantityLabelLanguage,
                            Value = quantityLabel.GetOrDefault("value", string.Empty),
                            Language = quantityLabelLanguage
                        } : null,
                        Id = unitId,
                        Amount = quantityValue.Get<string>("amount"),
                        Rank = rank
                    };
                case Sense.dataType:
                    var senseValue = dataValue.Get<JObject>("value");
                    var senseId = senseValue.Get<string>("id");
                    var sense = GetReferencedEntityById(senseId);
                    var senseGloss = sense.GetOrFirst("glosses", "en");
                    var senseGlossLanguage = senseGloss.GetOrDefault("language", string.Empty);
                    return new Sense
                    {
                        Id = senseId,
                        Glosses = new()
                        {
                            new Gloss
                            {
                                Id = senseGlossLanguage,
                                Value = senseGloss.GetOrDefault("value", string.Empty),
                                Language = senseGlossLanguage
                            }
                        },
                        EntityType = senseValue.Get<string>("entity-type"),
                        Rank = rank
                    };
                case WikidataClient.Model.Statement.Subjects.String.dataType:
                    return new WikidataClient.Model.Statement.Subjects.String
                    {
                        Value = dataValue.Get<string>("value"),
                        Rank = rank
                    };
                case TabularData.dataType:
                    return new TabularData
                    {
                        Value = dataValue.Get<string>("value"),
                        Rank = rank
                    };
                case Time.dataType:
                    var timeValue = dataValue.Get<JObject>("value");
                    var calenderModelId = timeValue.Get<string>("calendarmodel");
                    JObject timeLabel = null;
                    string timeLabelLanguage = null;
                    if (SetIdIfValid(ref calenderModelId))
                    {
                        calenderModelId = $"Q{calenderModelId.Split('Q')[1]}";
                        var calenderModelEntity = GetReferencedEntityById(calenderModelId);
                        timeLabel = calenderModelEntity.GetOrFirst("labels", "en");
                        timeLabelLanguage = timeLabel.GetOrDefault("language", string.Empty);
                    }
                    else
                    {
                        calenderModelId = Guid.NewGuid().ToString();
                    }
                    return new Time
                    {
                        Id = calenderModelId,
                        Label = timeLabel is not null ? new Label
                        {
                            Id = timeLabelLanguage,
                            Value = timeLabel.GetOrDefault("value", string.Empty),
                            Language = timeLabelLanguage
                        } : null,
                        Value = timeValue.Get<string>("time"),
                        Precision = timeValue.Get<int>("precision"),
                        Timezone = timeValue.Get<int>("timezone"),
                        Befor = timeValue.Get<int>("before"),
                        After = timeValue.Get<int>("after"),
                        Rank = rank
                    };
                case URL.dataType:
                    return new URL
                    {
                        Value = dataValue.Get<string>("value"),
                        Rank = rank
                    };
                default:
                    return new Unknown
                    {
                        Value = dataValue.Get<JObject>("value").ToString(),
                        Rank = rank
                    };
            }
        }

        private JObject GetReferencedEntityById(string id) =>
            _referencedEntities.SingleOrDefault(e => e.Get<string>("id") == id) ??
                _referencedEntities.Single(e => e.Get<string>("id") == _redirects[id]);

        private async Task<List<JObject>> GetReferencedEntities(string entityType, JObject jsonEntity, Dictionary<string, List<JObject>> claims)
        {
            var ids = new List<string>();

            foreach (var pair in claims)
            {
                ids.Add(pair.Key);

                foreach (var value in pair.Value)
                {
                    var mainsnak = value.Get<JObject>("mainsnak");
                    if (mainsnak.GetOrDefault<JObject>("datavalue") is JObject dataValue)
                    {
                        if(GetIdFromDataValue(mainsnak.Get<string>("datatype"), dataValue) is string id)
                        {
                            ids.Add(id);
                        }

                        if(value.GetOrDefault<JArray>("references") is JArray references) {
                            foreach (var referenceGroup in references) 
                            {
                                foreach (var snak in referenceGroup.GetToObject<Dictionary<string, JArray>>("snaks"))
                                {
                                    ids.Add(snak.Key);

                                    foreach (var reference in snak.Value)
                                    {
                                        if (reference.GetOrDefault<JObject>("datavalue") is JObject referenceDataValue)
                                        {
                                            if(GetIdFromDataValue(reference.Get<string>("datatype"), referenceDataValue) is string referenceId)
                                            {
                                                ids.Add(referenceId);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (value.ContainsKey("qualifiers"))
                        {
                            var qualifiers = value.GetToObject<Dictionary<string, List<JObject>>>("qualifiers");

                            foreach (var qualifier in qualifiers)
                            {
                                ids.Add(qualifier.Key);

                                foreach (var qual in qualifier.Value)
                                {
                                    if (qual.GetOrDefault<JObject>("datavalue") is JObject qualifierDataValue)
                                    {
                                        if (GetIdFromDataValue(qual.Get<string>("datatype"), qualifierDataValue) is string qualifierId)
                                        {
                                            ids.Add(qualifierId);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            var entities = new List<JObject>();
            var properties = new List<string> { "descriptions", "labels" };
            foreach (var idParameter in CreateIdParameters(ids.Distinct().ToList()))
            {
                var loadedEntities = await GetByIdsAsync(idParameter, properties);
                foreach (var entity in loadedEntities)
                {
                    if (entity.GetOrDefault<JObject>("redirects") is JObject redirects)
                    {
                        _redirects.Add(redirects.Get<string>("from"), redirects.Get<string>("to"));
                    }
                }

                entities.AddRange(loadedEntities);
            }

            return entities;
        }

        private string GetIdFromDataValue(string type, JObject dataValue) => type switch
        {
            Item.dataType => dataValue.Get<JObject>("value").Get<string>("id"),
            Property.dataType => dataValue.Get<JObject>("value").Get<string>("id"),
            Quantity.dataType => dataValue.Get<JObject>("value").Get<string>("unit") is string unit ?
                                    SetIdIfValid(ref unit) ?
                                        unit :
                                        null :
                                    null,
            Time.dataType => dataValue.Get<JObject>("value").Get<string>("calendarmodel") is string calendarmodel ?
                                    SetIdIfValid(ref calendarmodel) ?
                                        calendarmodel :
                                        null :
                                    null,
            Form.dataType => dataValue.Get<JObject>("value").Get<string>("id"),
            Lexeme.dataType => dataValue.Get<JObject>("value").Get<string>("id"),
            Sense.dataType => dataValue.Get<JObject>("value").Get<string>("id"),
            _ => null
        };

        private async Task<List<JObject>> GetByIdsAsync(string ids, List<string> properties = null)
        {
            var response = await _httpClient.GetStringAsync(GetUrl(ids, properties));

            return JObject.Parse(response)["entities"].ToObject<Dictionary<string, JObject>>().Values.ToList();
        }

        private List<string> CreateIdParameters(List<string> idList)
        {
            var idParameters = new List<string>();
            for (int i = 0; i < idList.Count; i += _parameterLenghtLimit)
            {
                idParameters.Add(GetListParameter(idList, i, idList.Count - i));
            }

            return idParameters;
        }

        private string GetUrl(string ids,
                      List<string> porperties = null) =>
                    $"https://www.wikidata.org/w/api.php?" +
                    $"action=wbgetentities&" +
                    $"ids={ids}&" +
                    (porperties is null ?
                            $"" :
                            $"props={GetListParameter(porperties)}&") +
                    $"format=json";

        private string GetListParameter<T>(List<T> list,
                                    int startIndex = 0,
                                    int limit = 0) =>
                    list?.GetRange(startIndex,
                                    Math.Min(_parameterLenghtLimit,
                                            limit < 1 ?
                                            list.Count :
                                            limit))
                    ?.ToString("|");

        private bool SetIdIfValid(ref string id) 
        {
            var tmp = id.Split("/").Last();
            if (_idPattern.IsMatch(tmp))
            {
                id = tmp;
                return true;
            }

            return false;
        }
    }
}
