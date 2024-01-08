using System;
using System.Reflection;

namespace Kethane.Scenarios
{
	// Token: 0x02000022 RID: 34
	public static class TutorialPageExtension
	{
		// Token: 0x060000E5 RID: 229 RVA: 0x00006B38 File Offset: 0x00004D38
		private static FieldInfo FindDialogField()
		{
			BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.NonPublic;
			foreach (FieldInfo fieldInfo in typeof(TutorialScenario.TutorialPage).GetFields(bindingAttr))
			{
				if (fieldInfo.FieldType == typeof(MultiOptionDialog))
				{
					return fieldInfo;
				}
			}
			return null;
		}

		// Token: 0x060000E6 RID: 230 RVA: 0x00006B85 File Offset: 0x00004D85
		public static MultiOptionDialog SetDialog(this TutorialScenario.TutorialPage page, MultiOptionDialog dialog)
		{
			TutorialPageExtension.dialogField.SetValue(page, dialog);
			return dialog;
		}

		// Token: 0x04000069 RID: 105
		private static FieldInfo dialogField = TutorialPageExtension.FindDialogField();
	}
}
