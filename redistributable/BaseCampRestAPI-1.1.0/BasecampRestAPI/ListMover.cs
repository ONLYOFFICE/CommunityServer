using System.Collections.Generic;

namespace BasecampRestAPI
{
	public class ListMover
	{
		public static ListMover GetInstance(IProject targetProject)
		{
			return new ListMover(targetProject);
		}
		private ListMover(IProject targetProject)
		{
			_targetProject = targetProject;
		}
		private readonly IProject _targetProject;
		private readonly Dictionary<int, IMilestone> _milestoneMap = new Dictionary<int, IMilestone>();
	}
}
