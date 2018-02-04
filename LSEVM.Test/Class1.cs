using System;

namespace VSThesis.Test
{    
    public class Class1
    {
        //public void MyMethod() {
        //    int a;
        //    int b;
        //    int c=1;
        //}

        public int MethodValueA(){
            return 1;
        }
        private int MethodB()
        {
            throw new NotImplementedException();
        }
        public int MethodValueB()
        {
            return 1;
        }
        private int MethodC()
        {
            throw new NotImplementedException();
        }

        public int XOverYTest()
        {
            int x = 0;
            int y = 0;
            x = MethodC();
            y = MethodB();
            if (x > 0 && x <= 3)
                return x / y;
            else return 0;
        }
       
        
        public void FloatTest(){

            //floating point supported up to one decimal.
            //float a = 3.0f;
            char a = 'a';
            //if (a == 152853.5f) {
            if (a == 'b')
            {
                throw new Exception();
            }
        }
        public void BoolTest(){

            bool a = false;
            if (a == true)
            {
                throw new Exception();
            }
        }
        public int AOverBTest(int a, int b){

            a = MethodValueA();
            b = MethodValueB();

            return a / b;
        }

        public int MyMethod_1(){//open parenthesis needs to be on line of method
            int a = 0;
            int b = 0;

            a = 0;
            a = MethodValueB();
            return a / b;
        }

        public int MyMethod_2()
        {
            int a = 0;
            int b = 0;

            a = MethodValueA();
            b = 0;
            return a / b;
        }

        public int MyMethod_3()
        {
            int a = 0;
            int b = 0;

            a = 0;
            b = 0;
            return a / b;
        }
    }
}
