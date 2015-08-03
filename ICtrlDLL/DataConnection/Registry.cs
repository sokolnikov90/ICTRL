using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ICtrlDLL
{
    class Registry
    {
        public const string REG_BASE = "SOFTWARE\\LANIT\\ICtrl\\VERSION300";

        public static bool GetValue(string _sSectionName, string _sKeyName, ref string _sValue)
        {
            Microsoft.Win32.RegistryKey key = null;

            try
            {
                key = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(_sSectionName);
                
                if (key == null)
                    return (false);

                _sValue = key.GetValue(_sKeyName).ToString();

                key.Close();
            }
            catch (Exception e)
            {
                ICtrl.logger.Info(e);
                return false;
            }
            finally
            {
                if (key != null)
                    key.Close();
            }

            return true;
        }

        public static bool GetDWORDValue(string _sSectionName, string _sKeyName, ref int _sValue)
        {
            Microsoft.Win32.RegistryKey key = null;

            try
            {
                key = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(_sSectionName);

                if (key == null)
                    return (false);

                _sValue = (int)key.GetValue(_sKeyName);

                key.Close();
            }
            catch (Exception e)
            {
                return false;
            }
            finally
            {
                if (key != null)
                    key.Close();
            }

            return true;
        }

        public bool SetValue(string _sSectionName, string _sKeyName, string _sValue)
        {
            Microsoft.Win32.RegistryKey key = null;

            try
            {
                key = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(_sSectionName);

                if (key == null)
                    return (false);

                key.SetValue(_sKeyName, _sValue);
                key.Close();
            }
            catch (Exception e)
            {
                return false;
            }
            finally
            {
                if (key != null)
                    key.Close();
            }

            return true;
        }

        public bool SetDWORDValue(string _sSectionName, string _sKeyName, int _sValue)
        {
            Microsoft.Win32.RegistryKey key = null;

            try
            {
                key = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(_sSectionName);

                if (key == null)
                    return false;

                key.SetValue(_sKeyName, _sValue);
                key.Close();
            }
            catch (Exception e)
            {
                return false;
            }
            finally
            {
                if (key != null)
                    key.Close();
            }

            return true;
        }
    }
}
