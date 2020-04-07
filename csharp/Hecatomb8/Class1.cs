using System;
using System.Collections.Generic;
using System.Text;

namespace Hecatomb8
{
    class Foo
    {
        public void Baz()
        {

        }
    }

    class Bar
    {
        public void Test()
        {
            var foo = new Foo();
            foo!.Baz();
        }
    }
}
