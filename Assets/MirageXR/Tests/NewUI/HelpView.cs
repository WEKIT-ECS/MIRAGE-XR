using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelpView : PopupBase
{
    protected override bool TryToGetArguments(params object[] args)
    {
        return true;
    }
}
