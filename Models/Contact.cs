using System;
using System.Collections.Generic;

namespace LLMOverlay.Models
{
    public class Contact
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string BaseModel { get; set; } = string.Empty;
        public string PhysicalDescription { get; set; } = string.Empty;
        public string Personality { get; set; } = string.Empty;
        public string Skills { get; set; } = string.Empty;
        public string SystemPrompt { get; set; } = string.Empty;
        public string AvatarPath { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime LastActive { get; set; }
        public Dictionary<string, object> ModelParameters { get; set; } = new Dictionary<string, object>();
        public string ApiEndpoint { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        
        public string AvatarDisplay 
        { 
            get 
            { 
                if (!string.IsNullOrEmpty(AvatarPath))
                {
                    return ""; // Will be handled by image binding
                }
                return "👤"; // Default emoji
            } 
        }

        public Contact()
        {
            Id = Guid.NewGuid().ToString();
            CreatedAt = DateTime.Now;
            LastActive = DateTime.Now;
            ModelParameters = new Dictionary<string, object>
            {
                { "Temperature", 0.7 },
                { "MaxTokens", 2048 },
                { "TopP", 0.9 },
                { "FrequencyPenalty", 0 },
                { "PresencePenalty", 0 }
            };
        }

        public void UpdateLastActive()
        {
            LastActive = DateTime.Now;
        }

        public Contact Clone()
        {
            return new Contact
            {
                Id = this.Id,
                Name = this.Name,
                BaseModel = this.BaseModel,
                PhysicalDescription = this.PhysicalDescription,
                Personality = this.Personality,
                Skills = this.Skills,
                SystemPrompt = this.SystemPrompt,
                AvatarPath = this.AvatarPath,
                CreatedAt = this.CreatedAt,
                LastActive = this.LastActive,
                ModelParameters = new Dictionary<string, object>(this.ModelParameters),
                ApiEndpoint = this.ApiEndpoint,
                ApiKey = this.ApiKey
            };
        }
    }
}