#Getting Started with FlexCel Studio for Xamarin

## 0. Before starting: A note on encodings

When you create a Xamarin iOS or Android application, by default it will come with a limited number of encodings.
This is to keep application size small. Those encodings include for example ASCII and Unicode, but no Win1252 (encoding used in 
western Windows machines) or IBM 437 (encoding used in zip files).

FlexCel will work in most cases with the reduced number of encodings, but there are some rare cases where we need the full list of encodings. An example is 
when reading an Excel 95 file, and there are a couple of other cases more.

So in order to not have problems with non existing encodings, it might be a good idea to click in your project properties and add "west" encodings.

* In iOS: Go to Build->iOS Build and select the "Advanced" tab. There in the "Internationalization" section choose "west".
* In Android: Go to Build->Android Build and select the "Linker" tab. There in the "Internationalization" section choose "west".

If you are worried about the extra size you can skip this step. FlexCel will still work in most of the cases without the extra encodings.

## 1. Creating an Excel file with code

The simplest way to use FlexCel is to use the XlsFile class to manipulate files:

```csharp
using System;
using FlexCel.Core;
using FlexCel.XlsAdapter;

namespace Samples
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            XlsFile xls = new XlsFile(1, TExcelFileFormat.v2016, true);
            xls.SetCellValue(1, 1, "Hello from FlexCel!");
            xls.SetCellValue(2, 1, 7);
            xls.SetCellValue(3, 1, 11);
            xls.SetCellValue(4, 1, new TFormula("=Sum(A2:A3)")); 
            xls.Save(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "test.xlsx"));
        }
    }
}
```

Note that this sample deduces the file format from the file name. If you saved as "test.xls", the file format would be xls, not xlsx.
You can specify the correct file format in a parameter to the "Save" method if needed; for example when saving to streams.

## 2. Creating a more complex file with code
While creating a simple file is simple (as it should), the functionality in Excel is quite big, and it can be hard to find out the exact method to do something.
FlexCel comes with a tool that makes this simpler:

#### 2.1 Download the correct APIMate for your operating system: 
  * [APIMate for OSX](http://www.tmssoftware.biz/flexcel/tools/net/ApiMate.dmg)
  * [APIMate for Windows](http://www.tmssoftware.biz/flexcel/tools/net/ApiMate.zip)
  
Or compile it from source (sources are included in this package).

#### 2.2. Create a file in Excel with the functionality you want.
o get the best results, keep the file simple. Say you want to find out how to add an autofilter, create an empty file in FlexCel and add an autofilter. If you want to find out how to format a cell with a gradient, create a different file and format one cell with a gradient.

Using simple files will make it much easier to find the relevant code in APIMate

#### 2.3. Open the file with APIMate
APIMate will tell you the code you need to recreate the file you created in Excel with FlexCel. You can see the code as C# or VB.NET. 

You can keep the xls/x file open in both Excel and APIMate, modify the file in Excel, save, press "Refresh" in APIMate to see the changes.

## 3. Reading a file
There is a complete example on Reading files in the documentation for the Windows installation. But for simple reading, you can use the following code:

```csharp

using System;
using FlexCel.Core;
using FlexCel.XlsAdapter;

namespace Samples
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            XlsFile xls = new XlsFile(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "test.xlsx"));

            xls.ActiveSheetByName = "Sheet1";  //we'll read sheet1. We could loop over the existing sheets by using xls.SheetCount and xls.ActiveSheet 
            for (int row = 1; row <= xls.RowCount; row++)
            {
                for (int colIndex = 1; colIndex <= xls.ColCountInRow(row); colIndex++) //Don't use xls.ColCount as it is slow: See Performance.Pdf
                {
                    int XF = -1;
                    object cell = xls.GetCellValueIndexed(row, colIndex, ref XF);

                    TCellAddress addr = new TCellAddress(row, xls.ColFromIndex(row, colIndex));
                    Console.Write("Cell " + addr.CellRef + " has ");
                    if (cell is TRichString) Console.WriteLine("a rich string.");
                    else if (cell is string) Console.WriteLine("a string.");
                    else if (cell is Double) Console.WriteLine("a number.");
                    else if (cell is bool) Console.WriteLine("a bool.");
                    else if (cell is TFlxFormulaErrorValue) Console.WriteLine("an error.");
                    else if (cell is TFormula) Console.WriteLine("a formula.");
                    else Console.WriteLine("Error: Unknown cell type");
                }
            }
        }
    }
}

```

## 4. Manipulating files

APIMate will tell you about a huge number of things, like how to paint a cell in red, or how to insert an autofilter.
But there are some methods that APIMate can't tell you about, and from those the most important are the manipulating methods:

Use **xls.InsertAndCopyRange** for inserting rows or column or ranges of cells. Also for copying ranges or cells or full rows or full columns.
Or for inserting and copying cells/columns/rows in one operation (like pressing "Copy/Insert copied cells" in Excel). 
It can also copy the cels from one sheet to the same sheet, to another sheet, or to another sheet in another file.
InsertAndCopyRange is a heavily overloaded method, and it can do many things depending on the parameters you pass to it.

Use **xls.DeleteRange** to delete ranges of cells, full rows or full columns.

Use **xls.MoveRange** to move a range, full rows or full columns from one place to another.

Use **xls.InsertAndCopySheets** to insert a sheet, to copy a sheet, or to insert and copy a sheet in the same operation.

Use **xls.DeleteSheet** to delete a sheet.

## 5. Creating Reports

You can create Excel files with code as shown above, but FlexCel also includes a reporting engine which uses Excel as the report designer. 
When using reports you create a template in Excel, write some tags on it, and run the report. 
FlexCel will replace those tags by the values from a database or memory.

#### 5.1. Create an empty file in Excel

#### 5.2. In cell A1 of the first sheet, write <#Customer.Name>. In cell B1 write <#Customer.Address>

#### 5.3. In the ribbon, go to "Formulas" tab, and press "Name manager" (In Excel for OSX or Excel 2003, go to Menu->Insert->Name->Define)

#### 5.3.1. Create a name "__Customer__" that refers to "=Sheet1!$A$1".  
The name is case insensitive, you can write it in any mix of upper and lower case letters.
It needs to start with two underscores ("\_") and end with two underscores too. We could use a single underscore for bands that don't take the full row or "I\_" or "I\_\_" for column reports instead, but this is for more advanced uses.


#### 5.4. Save the file as "report.template.xlsx"

#### 5.5. Write this code:

```csharp
using System;
using System.Collections.Generic;
using FlexCel.Core;
using FlexCel.Report;

namespace Samples
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            //Uncomment if in a console app: MonoMac.AppKit.NSApplication.Init();
            var Customers = new List<Customer>();
            Customers.Add(new Customer{ Name = "Bill", Address = "555 demo line" });
            Customers.Add(new Customer{ Name = "Joe", Address = "556 demo line" });

            using (FlexCelReport fr = new FlexCelReport(true))
            {
                fr.AddTable("Customer", Customers);
                fr.Run(
                    System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "report.template.xlsx"),
                    System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "result.xlsx")
                );
            }
        }
    }

    class Customer
    {
        public string Name { get; set; }
        public string Address { get; set; }
    }
```

##### 5.5.1. **Notes**. 
If doing an OSX Console application, you will need to add a reference to System.Data, System.Xml and XamMac or MonoMac
to the app. You might also need to copy XamMac.dll or MonoMac.dll you your output folder. And in a console application, you will
need to initialize the Cocoa framework by calling MonoMac.AppKit.NSApplication.Init()  (As commented in the code). 
For normal applications, you will probably not need to do anything.

## 6. Exporting a file to pdf
FlexCel offers a lot of options to export to pdf, like PDF/A, exporting font subsets, signing the generated pdf documents, etc. This is all shown in the examples and documentation. But for a simple export you can use the following code:

```csharp
using System;
using FlexCel.Core;
using FlexCel.XlsAdapter;
using FlexCel.Render;

namespace Samples
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            MonoMac.AppKit.NSApplication.Init();
 
            XlsFile xls = new XlsFile(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "result.xlsx"));

            using (var pdf = new FlexCelPdfExport(xls, true))
            {
                pdf.Export(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "result.pdf"));
            }
        }
    }
}
```

#### 6.1. The same notes in 5.5.1 apply here.

## 7. Exporting a file to html
As usual, there are too many options when exporting to html to show here: Exporting as html 3.2, 4 or 5, embedding images or css, exporting each sheet as a tab and a big long list of etc. And as usual, you can find all the options in the documentation and examples. 

For this getting started guide, we will show how to do an export with the default options of the active sheet.


```csharp
using System;
using FlexCel.Core;
using FlexCel.XlsAdapter;
using FlexCel.Render;

namespace Samples
{
    class MainClass
    {
        public static void Main(string[] args)
        { 
            XlsFile xls = new XlsFile(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "result.xlsx"));

            using (var html = new FlexCelHtmlExport(xls, true))
            {
                html.Export(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "result.html"), null);
            }
        }
    }
}
```

#### 7.1. The same notes in 5.5.1 apply here.

## 8.Browsing through the Examples
FlexCel comes with more than 50 examples on how to do specific things. Those demos are written for Visual Studio and Windows, but the concepts they show apply also to Xamarin.

So the next step to truly see what can be done is to install FlexCel for Visual Studio and open the main demo. You can open each demo as a standalone project, but you can also use the included "Demo Browser" (this is MainDemo.csproj) to look at them all in a single place. 

You can search for specific keywords at the top right of the main screen, to locate the demos that deal with specific features. So for example if you are looking for demos which show encryption, you will write "encrypt" in the search box.


## 9. This ends this small guide, but there is much more.
 To continue take a look at the over 50 examples in the main FlexCel distribution (currently available only for Windows), 
 and read the following documents (which are also available offline in the FlexCel for Windows setup):
 
* [What's new](http://www.tmssoftware.biz/flexcel/docs/net/whatsnew.htm)
* [Using the FlexCel API to read or write files](http://www.tmssoftware.biz/flexcel/docs/net/UsingFlexCelAPI.pdf)
* [Using FlexCel Reports to create files](http://www.tmssoftware.biz/flexcel/docs/net/UsingFlexCelReports.pdf)
* [Designing templates for FlexCel Reports](http://www.tmssoftware.biz/flexcel/docs/net/EndUserGuide.pdf)
* [Tags available in FlexCel Reports](http://www.tmssoftware.biz/flexcel/docs/net/FlexCelReportTags.xls)
* [Notes about performance](http://www.tmssoftware.biz/flexcel/docs/net/Performance.pdf)
* [Exporting files to pdf](http://www.tmssoftware.biz/flexcel/docs/net/UsingFlexCelPdfExport.pdf)
* [Exporting files to html](http://www.tmssoftware.biz/flexcel/docs/net/UsingFlexCelHTMLExport.pdf)
* [Using FlexCel in iOS](http://www.tmssoftware.biz/flexcel/docs/net/iOS.pdf)
* [A tutorial on how to work with files in iOS](http://www.tmssoftware.biz/flexcel/docs/net/FlexCelViewTutorial.pdf) 
* [Using FlexCel with Windows Phone and Windows Store](http://www.tmssoftware.biz/flexcel/docs/net/WinPhoneAndStore.pdf)
* [Using FlexCel with Android](http://www.tmssoftware.biz/flexcel/docs/net/Android.pdf)
* [Using FlexCel with Mono and Linux](http://www.tmssoftware.biz/flexcel/docs/net/FlexCelAndMono.pdf)
* [Functions supported in recalculation](http://www.tmssoftware.biz/flexcel/docs/net/SupportedFunctions.xls)
* [Autoshapes supported when exporting to pdf or html](http://www.tmssoftware.biz/flexcel/docs/net/SupportedAutoshapes.xls)
* [Deploying FlexCel](http://www.tmssoftware.biz/flexcel/docs/net/DEPLOYING_FLEXCEL.txt)
* [Configuring FlexCel](http://www.tmssoftware.biz/flexcel/docs/net/CONFIGURING_FLEXCEL.txt)
* [Copyright information](http://www.tmssoftware.biz/flexcel/docs/net/copyright.txt)
* [Online help](http://www.tmssoftware.biz/flexcel/hlp/net/index.aspx)


 
 
