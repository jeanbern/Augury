[![Stories in Ready](https://badge.waffle.io/jeanbern/Augury.png?label=ready&title=Ready)](https://waffle.io/jeanbern/Augury)
# Augury

[![Build status](https://ci.appveyor.com/api/projects/status/1lmiyf9319aagk6p?svg=true)](https://ci.appveyor.com/project/jeanbern/augury)

A small collection of natural language processing tools in C#. Augury is intended for use as a text predictor/spell-checker.  
Using a DAWG and the Jaro-Winkler distance, we evaluate possible word endings and spell-checks. These are then evaluated using Modified-Knesser-Ney smoothing, and the top results are returned. Support for symmetric-delete correction as an alternative is included.  
Most behavior is injectable and interfaces are provided to enable extension.  

Documentation coming soon.

---

The MIT License (MIT)

Copyright (c) 2016 Jean-Bernard Pellerin

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

