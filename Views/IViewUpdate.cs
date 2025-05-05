using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Views
{
    //View should be use to display and get the input 
    public interface IViewUpdate
    {

        void ShowSucessMessage(string message);

        void ShowErrorMessage(string message);

        void GetExpenseUserInput();



    }
}
