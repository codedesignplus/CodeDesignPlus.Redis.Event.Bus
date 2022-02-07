using CodeDesignPlus.Event.Bus.Abstractions;
using System;

namespace CodeDesignPlus.Redis.Event.Bus.Test.Helpers.Events
{
    /// <summary>
    /// User Created Event
    /// </summary>
    public class UserCreatedEvent: EventBase
    {
        /// <summary>
        /// Gets or sets the id
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// Gets or sets the user name
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// Gets or sets the names
        /// </summary>
        public string Names { get; set; }
        /// <summary>
        /// Gets or sets the last names
        /// </summary>
        public string Lastnames { get; set; }
        /// <summary>
        /// Gets or sets the birth date
        /// </summary>
        public DateTime Birthdate { get; set; }
    }
}
