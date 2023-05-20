using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.SemanticFunctions;
using NuGet.Protocol.Plugins;

namespace BostNex.Services.SemanticKernel
{

    /// <summary>
    /// 使用する可能性のあるSkillを登録する
    /// 取り敢えずジャンル分けしておく
    /// 
    /// ※現在の所、3.5のみ使用する。
    /// </summary>
    public interface ISkillPromptService
    {
        /// <summary>
        /// 存在するスキルを全て登録する。
        /// </summary>
        /// <param name="kernel"></param>
        public void RegisterAllSkill(IKernel kernel);
    }
    public enum SkillCategory
    {
        Test,
        DarkMagic,
        LightMagic
    }

    public enum DarkMagicFunction
    {
        Summarize
    }

    public class SkillPromptService : ISkillPromptService
    {
        public void RegisterAllSkill(IKernel kernel)
        {
            // いっそのこと、Skillsフォルダ以下のディレクトリスキャンして入れたら良くない？
            // →Webだとダメかも…。
            var skills = Enum.GetValues(typeof(SkillCategory))
                .Cast<SkillCategory>()
                .Select(s => s.ToString())
                .ToArray();

            var mySkill = kernel.ImportSemanticSkillFromDirectory("Skills", skills);
        }

    }

}
