using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Caddress.Template.Bridge {
    public interface IDecorate {

        void Show();

        void Hide();

        void Transparent();

        void Flash(Color color);

        void HighLight(Color color);

        bool IsShow();
    }
}