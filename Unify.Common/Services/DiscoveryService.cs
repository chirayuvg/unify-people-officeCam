using Microsoft.Extensions.Hosting;

namespace Unify.Common
{
    public class DiscoveryService
    {
        private readonly IHostEnvironment _hostEnvironment;
        public DiscoveryService(IHostEnvironment hostEnvironment)
        {
            _hostEnvironment = hostEnvironment;
        }

        public string GetVisionStudioUrl()
        {
            if (_hostEnvironment.IsDevelopment())
            {
                return "https://localhost:5100/dashboard";
            }
            if (_hostEnvironment.IsStaging())
            {
                return "https://stagingvs.unifynow.ai/dashboard";
            }
            if (_hostEnvironment.IsProduction())
            {
                return "https://visionstudio.unifynow.ai/dashboard";
            }
            return string.Empty;
        }

        public string GetWorkplaceUrl()
        {
            if (_hostEnvironment.IsDevelopment())
            {
                return "https://localhost:5000";
            }
            if (_hostEnvironment.IsStaging())
            {
                return "https://uatwp.unifynow.ai/";
            }
            if (_hostEnvironment.IsProduction())
            {
                return "https://workplacestudio.unifynow.ai/";
            }
            return string.Empty;
        }

        public string GetWorkplaceMonitorUrl()
        {
            if (_hostEnvironment.IsDevelopment())
            {
                return "http://localhost:5287";
            }
            if (_hostEnvironment.IsStaging())
            {
                return "https://uatdigitalworkplace.unifynow.ai/";
            }
            if (_hostEnvironment.IsProduction())
            {
                return "https://inspection.unifynow.ai/";
                //return "https://digitalworkplace.unifynow.ai/";
            }
            return string.Empty;
        }

        public string GetSelectApplicationUrl()
        {
            if (_hostEnvironment.IsDevelopment())
            {
                return "https://localhost:7119/";
            }
            if (_hostEnvironment.IsStaging())
            {
                return "https://uat.unifynow.ai/";
            }
            if (_hostEnvironment.IsProduction())
            {
                return "https://saas.unifynow.ai/";
            }
            return string.Empty;
        }

        public string GetSaaSWebUrl()
        {
            if (_hostEnvironment.IsDevelopment())
            {
                return "https://localhost:7119/Identity/Account/Login";
            }
            if (_hostEnvironment.IsStaging())
            {
                return "https://uatsaas.unifynow.ai/Identity/Account/Login";
            }
            if (_hostEnvironment.IsProduction())
            {
                return "https://saas.unifynow.ai/Identity/Account/Login";
            }
            return string.Empty;
        }

    }
}
