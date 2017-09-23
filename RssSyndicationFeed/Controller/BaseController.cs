﻿using RssSyndicationFeed.Configuration;
using RssSyndicationFeed.Configuration.Interface;
using RssSyndicationFeed.Model.Interface;
using RssSyndicationFeed.Model.SubContent;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace RssSyndicationFeed.Controller
{
    public abstract class BaseController
    {
        protected readonly IConstants config;

        public BaseController(IConstants config)
        {
            this.config = config;
        }

        public T DynamicElementLoad<T>(XElement element, T model) where T : class
        {
            if (!string.IsNullOrEmpty(element.Name.NamespaceName) && element.Name.NamespaceName != element.GetDefaultNamespace())
            {
                model = DynamicExtensionLoad(element, model);
                return model;
            }
            else
            {
                var type = config.ElementToType
                                      .Where(e => e.Key == element.Name.LocalName)
                                      .Select(e => e.Value)
                                      .FirstOrDefault();

                if(type == null)
                {
                    return model;
                }

                var newObject = (type == typeof(string)) ? string.Empty : Activator.CreateInstance(type);

                if (!element.HasElements)
                {
                    if (element.HasAttributes)
                    {
                        newObject = DynamicAttributeLoad(element, newObject);
                    }

                    if (!element.IsEmpty)
                    {
                        newObject = SetPropertyValue(config.ElementValueToProperty, element.Name.LocalName, element.Value, newObject);
                    }
                    model = SetPropertyValue(config.ElementToProperty, element.Name.LocalName, newObject, model);
                    return model;
                }

                foreach (var subElement in element.Elements())
                {
                    newObject = DynamicElementLoad(subElement, newObject);
                }

                model = SetPropertyValue(config.ElementToProperty, element.Name.LocalName, newObject, model);

                return model;
            }
        }

        public T DynamicAttributeLoad<T>(XElement element, T model) where T : class
        {
            foreach (var attr in element.Attributes())
            {
                var type = config.AttributeToType
                                      .Where(e => e.Key == attr.Name.LocalName)
                                      .Select(e => e.Value)
                                      .FirstOrDefault();

                if (type == null)
                {
                    continue;
                }

                var newObject = (type == typeof(string)) ? string.Empty : Activator.CreateInstance(type);

                newObject = SetPropertyValue(config.AttributeValueToProperty, attr.Name.LocalName, attr.Value, newObject);
                model = SetPropertyValue(config.AttributeToProperty, attr.Name.LocalName, newObject, model);
            }

            return model;
        }

        public T SetPropertyValue<T>(IDictionary<string, string> collection, string name, object value, T model) where T : class
        {
            var propertyName = collection
                                             .Where(e => e.Key == name)
                                             .Select(e => e.Value)
                                             .FirstOrDefault();

            if (value.GetType() == typeof(string) && string.IsNullOrEmpty(propertyName))
            {
                model = value as T;
                return model;
            }

            var property = model.GetType()
                                   .GetProperty(propertyName);

            if (property.PropertyType.IsGenericType)
            {
                
                var propertyValue = property.GetValue(model);

                propertyValue = (propertyValue == null) ? Activator.CreateInstance(property.PropertyType) : propertyValue;

                propertyValue.GetType()
                             .GetMethod("Add")
                             .Invoke(propertyValue, new[] { value });

                property.SetValue(model, propertyValue);

            }
            else
            {
                property.SetValue(model, value);
            }

            return model;
        }

        public T DynamicExtensionLoad<T>(XElement element, T model)
        {
            var property = model.GetType()
                                               .GetProperty("Extensions");

            if(property == null)
            {
                return model;
            }

            var extensions = property.GetValue(model);

            extensions = (extensions == null) ? Activator.CreateInstance(property.PropertyType) : extensions;

            var extension = ((IEnumerable)extensions).Cast<dynamic>()
                                                     .Where(e => e.Namespace == element.Name.NamespaceName)
                                                     .FirstOrDefault();

            if (extension == null)
            {
                extension = RssSyndicationExtensionManager.Create(element);
                extensions.GetType().GetMethod("Add").Invoke(extensions, new[] { extension });
                property.SetValue(model, extensions);
            }

            var formatter = GlobalConstants.SupportedFormatters
                                          .Where(e => e.Key == extension.Name)
                                          .Select(e => e.Value)
                                          .FirstOrDefault();

            if (formatter != null)
            {
                var formatterObject = Activator.CreateInstance(formatter);

                var bootstrapper = GlobalConstants.BootstrapMethods
                                                  .Where(e => e.Key == extension.Name)
                                                  .Select(e => e.Value)
                                                  .FirstOrDefault();

                formatter.GetMethod(bootstrapper)
                         .Invoke(formatterObject, new object[] { extension, element });
            }

            extension.Description = element.ToString();

            return model;
        }

        public T DynamicExtensionElementLoad<T>(XElement element, T model) where T : class
        {
            var type = config.ElementToType
                                      .Where(e => e.Key == element.Name.LocalName)
                                      .Select(e => e.Value)
                                      .FirstOrDefault();

            var newObject = type == null ? model : (type == typeof(string)) ? string.Empty : Activator.CreateInstance(type);

            if (!element.HasElements)
            {
                if (element.HasAttributes)
                {
                    newObject = DynamicAttributeLoad(element, newObject);
                }

                if (!element.IsEmpty)
                {
                    newObject = SetPropertyValue(config.ElementValueToProperty, element.Name.LocalName, element.Value, newObject);
                }
                model = SetPropertyValue(config.ElementToProperty, element.Name.LocalName, newObject, model);
                return model;
            }

            foreach (var subElement in element.Elements())
            {
                newObject = DynamicExtensionElementLoad(subElement, newObject);
            }

            model = type == null ? model : SetPropertyValue(config.ElementToProperty, element.Name.LocalName, newObject, model);

            return model;
        }
    }
}
