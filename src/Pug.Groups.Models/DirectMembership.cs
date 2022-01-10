using System;

namespace Pug.Groups.Models
{
	public class DirectMembership : Membership
	{
		public DateTime AssignmentTimestamp { get; set; }
		
		public string Assignor { get; set; }
	}
}