using UnityEngine.ResourceManagement.ResourceLocations;

namespace Shirahama;

public interface IRuntimeResolver
{
	string Resolve(IResourceLocation location);
}
