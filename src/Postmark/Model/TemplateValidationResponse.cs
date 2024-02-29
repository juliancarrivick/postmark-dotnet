﻿using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace PostmarkDotNet
{
    /// <summary>
    /// Indicates the result of validating and test rendering template content against the API.
    /// </summary>
    public class TemplateValidationResponse
    {
        /// <summary>
        /// Indicates whether all of the parts included in the validation request are valid and rendered properly using a test model.
        /// </summary>
        public bool AllContentIsValid { get; set; }

        /// <summary>
        /// If HTMLBody was present in validation request, indicates the outcome of the validation/rendering
        /// </summary>
        public TemplateValidationResult HtmlBody { get; set; }

        /// <summary>
        /// If TextBody was present in validation request, indicates the outcome of the validation/rendering
        /// </summary>
        public TemplateValidationResult TextBody { get; set; }

        /// <summary>
        /// If Subject was present in validation request, indicates the outcome of the validation/rendering
        /// </summary>
        public TemplateValidationResult Subject { get; set; }

        private dynamic _suggestedModel = null;

        /// <summary>
        /// The merged request model, with any additional values that are referenced by any of the supplied templates.
        /// </summary>
        public dynamic SuggestedTemplateModel { get { return _suggestedModel; } set { _suggestedModel = ConvertJsonResponse(value); } }

        private dynamic ConvertJsonResponse(dynamic value)
        {
            dynamic retval = null;
            if (value is JsonElement elem)
            {
                switch (elem.ValueKind)
                {
                    case JsonValueKind.Object:
                        value = JsonObject.Create(elem);
                        break;
                    case JsonValueKind.Array:
                        value = JsonArray.Create(elem);
                        break;
                    default:
                        value = JsonValue.Create(elem);
                        break;
                }
            }
            if (value is JsonObject obj)
            {
                var dictionary = new ExpandoObject() as IDictionary<string, object>;
                foreach (var prop in obj)
                {
                    dictionary[prop.Key] = ConvertJsonResponse(prop.Value);
                }
                retval = dictionary as ExpandoObject;
            }
            else if (value is JsonArray arr)
            {
                retval = arr.Select(ConvertJsonResponse).ToArray();
            }
            else if (value is JsonValue val)
            {
                retval = val.GetValueKind() switch
                {
                    JsonValueKind.False => false,
                    JsonValueKind.True => true,
                    JsonValueKind.Null => null,
                    JsonValueKind.Number => val.GetValue<double>(),
                    _ => val.ToString()
                };
            }
            return retval;
        }

        /// <summary>
        /// Indicates the outcome of validation of a given template.
        /// </summary>
        public class TemplateValidationResult
        {
            public TemplateValidationResult()
            {
                ValidationErrors = new TemplateValidationError[0];
            }

            public bool ContentIsValid { get; set; }
            public IEnumerable<TemplateValidationError> ValidationErrors { get; set; }
            public string RenderedContent { get; set; }

            public class TemplateValidationError
            {
                public string Message { get; set; }
                public int? Line { get; set; }
                public int? CharacterPosition { get; set; }
            }
        }

    }
}