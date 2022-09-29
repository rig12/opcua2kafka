using AutoMapper;
using GPNA.OPCUA2Kafka.Configurations;
using GPNA.Extensions;
using GPNA.Repository;
using GPNA.OPCUA2Kafka.Interfaces;
using Microsoft.Extensions.Logging;
using GPNA.OPCUA2Kafka.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using GPNA.Templates.Constants;
using GPNA.OPCUA2Kafka.Extensions;

namespace GPNA.OPCUA2Kafka.Services
{
    /// <summary>
    /// Реализация класса управления конфигуарциями тегов
    /// </summary>
    public class TagConfigurationManager : ITagConfigurationManager
    {
        #region Constructors
        public TagConfigurationManager(IRepositoryFactory repositoryFactory, IMapper mapper, ILogger<TagConfigurationManager> logger)
        {
            _repositoryFactory = repositoryFactory;
            _mapper = mapper;
            _logger = logger;
            _source = newTagConfigurationDictionary();
            Load();
        }
        #endregion Constructors


        #region Fields
        private readonly IMapper _mapper;
        private readonly ILogger<TagConfigurationManager> _logger;
        private readonly IRepositoryFactory _repositoryFactory;
        private ConcurrentDictionary<string, TagConfigurationEntity> _source;
        private readonly ConcurrentDictionary<string, string> _shortnames = new();

        #endregion Fields


        #region Properties
        public bool IsLoaded { get; private set; }


        public IDictionary<string, TagConfigurationEntity> TagConfigurations
        {
            get
            {
                return _source;
            }
        }

        private ConcurrentDictionary<string, TagConfigurationEntity> newTagConfigurationDictionary()
        {
            return new ConcurrentDictionary<string, TagConfigurationEntity>(StringComparer.InvariantCultureIgnoreCase);
        }
        #endregion Properties


        #region Methods 
        #region Common
        public bool Load()
        {
            try
            {
                _source = newTagConfigurationDictionary();
                using var repository = _repositoryFactory.GetRepository<TagConfigurationEntity>();
                foreach (var item in repository.GetAll())
                {
                    var key = (item as ITagConfiguration)?.ConvertToString() ?? throw new NullReferenceException();
                    if (!_source.ContainsKey(key))
                    {
                        _source.TryAdd(key, _mapper.Map<TagConfigurationEntity>(item));
                    }
                    else
                    {
                        throw new ArgumentException("An item with the same key has already been added");
                    }
                    //repository.Update(item);
                }
                IsLoaded = true;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception.ToString());
                IsLoaded = false;
            }
            return IsLoaded;
        }

        public TagConfigurationEntity? GetByAlias(string alias)
        {
            if (_source.TryGetValue(alias, out var result))
            {
                return result;
            }
            return default;
        }


        public TagConfigurationEntity? GetByTagName(string tagName)
        {
            if (_source.TryGetValue(tagName, out var result))
            {
                return result;
            }
            return default;
        }


        public string? ShortenName(string tagname)
        {
            if (!string.IsNullOrEmpty(tagname))
            {
                if (!_shortnames.TryGetValue(tagname, out var tagshortname))
                {
                    var newtagshortname = tagname.Split('!') is string[] tagnameParts && tagnameParts.Length > 1
                        ? tagnameParts[1]
                        : tagname;
                    _shortnames.AddOrUpdate(tagname, newtagshortname, (key, value) => value);
                    return newtagshortname;
                }
                else
                {
                    return tagshortname;
                }
            }
            return tagname;
        }
        #endregion Common


        #region Add
        
        public IEnumerable<TagConfigurationEntity> Add(IEnumerable<TagConfiguration> tagConfigurations)
        {
            var entities = _mapper.Map<IEnumerable<TagConfigurationEntity>>(tagConfigurations);
            var keys = new List<string>();
            var result = new List<TagConfigurationEntity>();
            foreach (var entity in entities)
            {
                var key = entity.ConvertToString();
                
                if (_source.ContainsKey(key))
                {
                    throw new ArgumentException(MessageConstants.ENTITY_ALREADY_EXISTS_TEXT);
                }
                if(!ValidateTopic(entity.Topic))
                {
                    throw new ArgumentException(MessageConstants.ENTITY_INCORRECT_TOPIC_TEXT);
                }
                 keys.Add(key);
            }
            using var repository = _repositoryFactory.GetRepository<TagConfigurationEntity>();
            foreach (var entity in entities)
            {
                var saved = repository.Add(entity);
                _source.TryAdd(keys[result.Count], saved);
                result.Add(saved);
            }
            return result;
        }
        #endregion Add

        #region Delete
        public bool Delete(long id)
        {
            var pair = _source.FirstOrDefault(x => x.Value.Id == id);
            if (pair.Value == default)
            {
                return false;
            }
            using var repository = _repositoryFactory.GetRepository<TagConfigurationEntity>();
            if (!repository.Delete(id))
            {
                return false;
            }
            return _source.TryRemove(pair.Key, out _);
        }
        #endregion Delete

        #region Update
        public bool Update(TagConfigurationEntity tagConfiguration)
        {
            var previous = _source.FirstOrDefault(x => x.Value.Id == tagConfiguration.Id);
            if (previous.Value == default)
            {
                return false;
            }
            var key = tagConfiguration.ConvertToString();
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException(MessageConstants.ENTITY_INCORRECT_FORMAT_TEXT);
            }
            if (_source.FirstOrDefault(x => x.Key == key && x.Value.Id != tagConfiguration.Id).Value != default)
            {
                throw new ArgumentException(MessageConstants.ENTITY_ALREADY_EXISTS_TEXT);
            }

            using var repository = _repositoryFactory.GetRepository<TagConfigurationEntity>();
            if (!repository.Update(tagConfiguration))
            {
                return false;
            }
            return _source.TryRemove(previous.Key, out _) && _source.TryAdd(key, tagConfiguration);
        }
        #endregion Update

        #region private


        private static bool ValidateTopic(string topic)
        {
            if (!string.IsNullOrWhiteSpace(topic))
            {
                try
                {
                    var match = Regex.Match(topic, "[a-zA-Z0-9\\._\\-]");
                    return match.Success;
                }
                catch { }
            }

            return false;
        }



        #endregion

        #endregion Methods
    }
}
