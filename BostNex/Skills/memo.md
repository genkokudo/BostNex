

# �쐬�����֐��̎擾���@
```
var summarizeFunc = kernel.Func(SkillCategory.DarkMagic.ToString(), DarkMagicFunction.Summarize.ToString());
var skills = kernel.Skills; // �o�^�����X�L���͂����������B
var view = skills.GetFunctionsView();   // �o�^�ꗗ���擾�ł���B
var aaaa = skills.GetFunction(SkillCategory.DarkMagic.ToString(), DarkMagicFunction.Summarize.ToString());
var bbbb = skills.GetFunction("bbbbfunctionName");        // �O���[�o���֐��̎擾�́AskillName���w�肵�Ȃ��B
```
Semantic�ȊO�ɁANative������B����́AC#�̏��������񂾊֐��B


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

