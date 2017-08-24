
using System;
using System.Collections.Specialized;
using System.Net;
using System.Net.Configuration;
using System.Reflection;

namespace BuildDemangledOutput
{
    public class CppDemangler
    {
        // Copy-pasted from https://stackoverflow.com/questions/8424144/how-to-set-useunsafeheaderparsing-in-code
        // Enable/disable useUnsafeHeaderParsing.
        // See http://o2platform.wordpress.com/2010/10/20/dealing-with-the-server-committed-a-protocol-violation-sectionresponsestatusline/
        public static bool ToggleAllowUnsafeHeaderParsing(bool enable)
        {
            //Get the assembly that contains the internal class
            Assembly assembly = Assembly.GetAssembly(typeof(SettingsSection));
            if (assembly != null)
            {
                //Use the assembly in order to get the internal type for the internal class
                Type settingsSectionType = assembly.GetType("System.Net.Configuration.SettingsSectionInternal");
                if (settingsSectionType != null)
                {
                    //Use the internal static property to get an instance of the internal settings class.
                    //If the static instance isn't created already invoking the property will create it for us.
                    object anInstance = settingsSectionType.InvokeMember("Section",
                    BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.NonPublic, null, null, new object[] { });
                    if (anInstance != null)
                    {
                        //Locate the private bool field that tells the framework if unsafe header parsing is allowed
                        FieldInfo aUseUnsafeHeaderParsing = settingsSectionType.GetField("useUnsafeHeaderParsing", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (aUseUnsafeHeaderParsing != null)
                        {
                            aUseUnsafeHeaderParsing.SetValue(anInstance, enable);
                            return true;
                        }

                    }
                }
            }
            return false;
        }

        public static string Demangle(string mangled)
        {
            // Enable UseUnsafeHeaderParsing
            if (!ToggleAllowUnsafeHeaderParsing(true))
            {
                // Couldn't set flag. Log the fact, throw an exception or whatever.
            }

            using (var webClient = new WebClient())
            {
                const string demanglerUrl = "https://demangler.com/raw";
                const string requestType = "POST";
                var request = new NameValueCollection { { "input", mangled } };

                try
                {
                    var response = webClient.UploadValues(demanglerUrl, requestType, request);
                    var responseAsString = System.Text.Encoding.UTF8.GetString(response);
#if DEBUG
                    Console.WriteLine(responseAsString);
#endif
                    return responseAsString;
                }
                catch (WebException ex)
                {
#if DEBUG
                    Console.WriteLine(ex.Message);
#endif
                    throw;
                }
            }
        }
    }
}
