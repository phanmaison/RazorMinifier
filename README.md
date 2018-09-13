# Razor Minifier

Thank you for getting here and show your interest in optimization.

### Why minifying the Razor View?

There are some good reasons to minify the Razor View output:
- Reduce the html size (about 20%)
- Make the output look professional
- **Remove all business comments which are not necessary on production env**

### How to

There are arguments (e.g. [ASPNET Razor](https://github.com/aspnet/Razor/issues/423)) on Razor View optimization, and (in my opinion) Microsoft is the best one to provide the solution, but it seems that they won't.  
So we must do some trick to make it work.  
  
The first solution is provided by [Dean](https://github.com/deanhume/html-minifier/) where you can minify the view when you publish your website. Yes, thanks Dean for providing the solution. However, there is problem that you can only verify your work after you publish the website, means that your development environment and test environment may not be the same (I faced this).  
- Note that the minifying is just a trick so it is difficult to cover all cases, hence there can be potential issues and need your effort to re-test the whole application over and over.  
  
That's why I would recommend you to try second approach: replace the MVC parser by your own so that you may have the same environment for development and testing, you can test both your work and minifying output right after you finish. Further explanation will be given in next section.

### How it work

More detailed explanation on how Razor work. You may refer to the open source of [ASPNET](https://github.com/aspnet/AspNetWebStack) for further understanding.
- You may need to understand the MVC request lifecycle first, please google it
- The request will be gone through Routing, executing the controller action
- If the ViewResult is return, MVC will execute it by calling the Parser (on the first time) to parse the Razor view and compile it
- Well done ASPNET team, they allows us to inject our own parser by overriding the RazorHostFactory (`MvcWebRazorHostFactory` by default)
- There are 2 parsers: HtmlParser to parse the html content and CodeParser to parse the code (C# or VB). HtmlParser will be run first
- So all the things we need to do is simply to override the HtmlParser, minify the View content before returning to the normal flow

### Usage

**For the host factory solution:**  
- Under each Views folder, there is a web.config file
- Replace the default `MvcWebRazorHostFactory` by the overrided `RazorMinificationHostFactory`, so the original line  
~~~XML
<host factoryType="System.Web.Mvc.MvcWebRazorHostFactory, System.Web.Mvc, Version=x.x.x.x, Culture=neutral, PublicKeyToken=XXXXX" /> 
~~~
is changed to  
~~~XML
<host factoryType="SampleWeb.RazorMinificationHostFactory" />
~~~
- Remember to replace in each Views folder, change the namespace and you are done
- Run and try the output  


**For the publish solution:**
- Builde the RazorMinifier.Console and copy RazorMinifier.exe to C:\ (or D:\)
- Create a publish profile using FileSystem method
- Open Properties\PublishProfiles\(Your Publish Profile Name).pubxml
- Add following lines
~~~XML
  <!-- Minify the Razor -->
  <Target Name="CustomAction" AfterTargets="CopyAllFilesToSingleFolderForPackage">
    <Message Text="Minifying files....." />
    <Exec Command="C:\RazorMinifier.exe $(_PackageTempDir)" IgnoreExitCode="true" />
  </Target>
~~~
- Optional arguments are: 
    - `ignorehtmlcomments`
    - `ignorejscomments`
    - `ignoreregion`
    - `ignoreknockoutcomments`
    - `maxlength=XXX`
- The RazorMinifier will minify the Views before copying to published folder  

Sample code is provided for your reference

### Note
Enjoy your output.  
As mentioned, the trick cannot cover all cases so if you found any issue, please report (and recommend solution if any).  
I have faced cases where unminified Views work but minified don't, such as:  
~~~javascript
<script>
    var option = { 
    }
    // please correctly append ;
    var app = {
    }
</script>
~~~

 

