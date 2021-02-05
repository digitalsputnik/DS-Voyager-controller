using System.IO; 
using UnityEngine; 
using UnityEditor; 
using UnityEditor.Build; 
using UnityEditor.Build.Reporting; 
using UnityEditor.iOS.Xcode;


namespace VoyagerController.Editor
{
    public class IOS142Workaround : IPostprocessBuildWithReport 
    {
        public int callbackOrder => 1;

        public void OnPostprocessBuild(BuildReport report)
        {
            #if UNITY_IOS
            if (report.summary.platform != BuildTarget.iOS) return;
            Debug.Log("Applying iOS 14.2 workaround. Remove me once Unity has patched this."); 
            var pathToBuiltProject = report.summary.outputPath; 
            var projectPath = PBXProject.GetPBXProjectPath(pathToBuiltProject); 
            var project = new PBXProject(); 
            project.ReadFromString(File.ReadAllText(projectPath)); 
            project.AddFrameworkToProject( project.GetUnityMainTargetGuid(), "UnityFramework.framework", false ); 
            project.WriteToFile( projectPath );
            #endif
        }
    }   
}