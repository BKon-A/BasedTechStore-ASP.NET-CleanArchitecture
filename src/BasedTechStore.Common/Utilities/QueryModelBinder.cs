using BasedTechStore.Common.ViewModels.Products;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasedTechStore.Common.Utilities
{
    public class QueryModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            var modelType = bindingContext.ModelType;
            var modelInstance = Activator.CreateInstance(modelType);

            var properties = modelType.GetProperties();

            foreach (var property in properties)
            {
                var camelCaseName = char.ToLowerInvariant(property.Name[0]) + property.Name.Substring(1);
                var valueProviderResult = bindingContext.ValueProvider.GetValue(camelCaseName);
                if (valueProviderResult == ValueProviderResult.None)
                    continue;

                var value = valueProviderResult.FirstValue;
                if (string.IsNullOrEmpty(value))
                    continue;

                try
                {
                    if (valueProviderResult != ValueProviderResult.None)
                    {
                        if (property.PropertyType.IsGenericType &&
                            property.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                        {
                            var elementType = property.PropertyType.GetGenericArguments()[0];
                            var listType = typeof(List<>).MakeGenericType(elementType);
                            var listInstance = Activator.CreateInstance(listType);
                            var addMethod = listType.GetMethod("Add");

                            foreach (var item in valueProviderResult.Values)
                            {
                                var convertedItem = Convert.ChangeType(item, elementType);
                                addMethod?.Invoke(listInstance, new[] { convertedItem });
                            }

                            property.SetValue(modelInstance, listInstance);
                        }
                        else
                        {
                            var convertedValue = Convert.ChangeType(value, Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType);
                            property.SetValue(modelInstance, convertedValue);
                        }
                    }
                }
                catch(Exception ex)
                {
                    bindingContext.ModelState.AddModelError(property.Name, $"Error binding property '{property.Name}': {ex.Message}");
                }
            }

            bindingContext.Result = ModelBindingResult.Success(modelInstance);
            return Task.CompletedTask;
        }
    }

    public class QueryModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            // Bind for entities that require complex query parameters
            if (context.Metadata.ModelType == typeof(ProductFilterVM))
                return new QueryModelBinder();

            return null;
        }
    }
}
