using System.Net;

namespace VoyagerApp.Projects
{
    public interface IProjectParser
    {
        ProjectSaveData Parse(string json);
    }
}
