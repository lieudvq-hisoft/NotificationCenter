﻿using System;
using MongoDB.Bson.Serialization.Attributes;

namespace Data.Entities
{
	public class BaseEntity
	{
        [BsonId]
        public Guid Id { get; set; } = Guid.NewGuid();
        public bool IsDeleted { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.Now;
        public DateTime DateUpdated { get; set; } = DateTime.Now;
    }
}

