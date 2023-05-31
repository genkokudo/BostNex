using BostNex.Skills;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.CoreSkills;

namespace BostNex.Services.SemanticKernel
{

    /// <summary>
    /// 使用する可能性のあるSkillを登録する
    /// 取り敢えずジャンル分けしておく
    /// 
    /// 現在の所、3.5のみ使用する。
    /// コード生成はAzureの4を使ってしまおう
    /// </summary>
    public interface ISkillPromptService
    {
        /// <summary>
        /// 存在するスキルを全て登録する。
        /// </summary>
        /// <param name="kernel"></param>
        public void RegisterAllSkill(IKernel kernel);
    }
    public enum SemanticSkillCategory
    {
        Test,
        DarkMagic,
        SkillMaker,
    }
    public enum NativeSkillCategory
    {
        LightMagic,
        Geed,

        // 使う可能性があるプリセットスキル
        TextMemory
    }

    public enum DarkMagicFunction
    {
        Summarize
    }

    public enum SkillMakerFunction
    {
        EnumeratingFunctionElements,
        GenerateNativeFunction
    }

    public class SkillPromptService : ISkillPromptService
    {
        public void RegisterAllSkill(IKernel kernel)
        {
            // SemanticSkillCategoryに挙げたSkillsフォルダ以下のスキルを全てインポートする
            var skills = Enum.GetValues(typeof(SemanticSkillCategory))
                .Cast<SemanticSkillCategory>()
                .Select(s => s.ToString())
                .ToArray();
            var mySkill = kernel.ImportSemanticSkillFromDirectory("Skills", skills);

            // ネイティブスキルも入れてみよう
            kernel.ImportSkill(new LightMagicSkill(), NativeSkillCategory.LightMagic.ToString());
            kernel.ImportSkill(new GeedSkill(), NativeSkillCategory.Geed.ToString());

            // プリセットスキルも入れてみよう
            kernel.ImportSkill(new TextMemorySkill(), NativeSkillCategory.TextMemory.ToString());
        }

    }

}
