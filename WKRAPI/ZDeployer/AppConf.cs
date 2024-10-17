namespace ZDeployer;

internal class AppConf
{
    public required string WkrPath { get; set; }

    public string ApiPath { get { return $"{WkrPath}\\WKRAPI"; } }
    public string UiPath { get { return $"{WkrPath}\\wkrui-vite"; } }
    //public string DashboardUiPath { get { return $"{WkrPath}\\WKRDashboard"; } }
    public string CordovaPath { get { return $"{WkrPath}\\WKRCordova"; } }

    public required string ApiDeploymentPath { get; set; }
    public required string UiDeploymentPath { get; set; }
    public required string ApkDeploymentPath { get; set; }
}
