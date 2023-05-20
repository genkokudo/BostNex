

# 作成した関数の取得方法
```
var summarizeFunc = kernel.Func(SkillCategory.DarkMagic.ToString(), DarkMagicFunction.Summarize.ToString());
var skills = kernel.Skills; // 登録したスキルはここから取れる。
var view = skills.GetFunctionsView();   // 登録一覧を取得できる。
var aaaa = skills.GetFunction(SkillCategory.DarkMagic.ToString(), DarkMagicFunction.Summarize.ToString());
var bbbb = skills.GetFunction("bbbbfunctionName");        // グローバル関数の取得は、skillNameを指定しない。
```
Semantic以外に、Nativeがある。これは、C#の処理を挟んだ関数。


```
{
  "schema": 1,
  "type": "completion",
  "description": "A sample prompt template configuration",
  "completion": {
    "temperature": 0.7,
    "top_p": 0.9,
    "presence_penalty": 0.1,
    "frequency_penalty": 0.2,
    "max_tokens": 512,
    "stop_sequences": ["\n\n", "==END=="]
  },
  "default_services": ["openai", "gpt-3", "another-service"],
  "input": {
    "parameters": [
      {
        "name": "input",
        "description": "This parameter contains the input text",
        "defaultValue": "default text"
      },
      {
        "name": "style",
        "description": "This parameter specifies the desired style",
        "defaultValue": "plain"
      }
    ]
  }
}
```

