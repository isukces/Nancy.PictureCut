Nancy.PictureCut
================

[PictureCut](http://picturecut.tuyoshi.com.br/) Wrapper for NancyFX framework


## PictureCut wrapper for NancyFx

`Nancy.PictureCut` is just port of Picture Cut - "jquery plugin that handles images in a very friendly and simple way". 
This package is originally designed for PHP based host. 
`Nancy.PictureCut` allows to use almost the same functionality in Nancy based application, and helps with boring steps

* contains all PictureCut files like javascript, images, templates and styles,
* provides module that make this files available by http requests,
* provides easy to configure wrapper

#### Links
* PictureCut project homepage: http://picturecut.tuyoshi.com.br/



## Step by step tutorial
Here is short checklist.  

* Create Nancy application. Refer Twitter Bootstrap and `Nancy.PictureCut`
* Create module with logic for handling images (i.e. store user photo)
* Create layout page (base for views) with base javascript and css links
* Create view.  with 
	* additional javascript
    * visual container for dropping images 


#### Create Nancy application
Create Nancy application based on Twitter Bootstrap. `Nancy.PictureCut` currently supports only this styling system.
#### Style sheets and javascript

Page layout for Twitter Bootstrap based application should contain something like this below.

In `head` section:

```
 <link href="~/Content/Site.css" rel="stylesheet" />
 <link href="~/Content/bootstrap.css" rel="stylesheet" />
 <link href="~/Content/bootstrap-theme.css" rel="stylesheet" />
 <script src="~/Scripts/jquery-2.1.0.js"></script>
 <script src="~/Scripts/modernizr-2.7.2.js"></script>
 <script src="~/Scripts/respond.js"></script>
```

and the bottom of the page, just above `</body>` tag:

```
 <script src="~/Scripts/bootstrap.js"></script>
 ```
 
Page layout is usually taken from `Layout.cshtml` file or similar. If you don't prefer to use layouts just copy all of this code into your view file.
 
#### PictureCut static content

`Nancy.PictureCut` contains `StaticContentModule` class that provides files required for PictureCut engine: javascript, images and templates. 
For example `window.pc.js` file (one of PictureCut scripts) can be accessed under http://somedomain.com/ImageCut/window.pc.js address.
Server path for this content can be set by `StaticContentModule.BaseUrl` static property. Default value is `/ImageCut/`. This value has been used in the example.

#### Create module

Usually you need to create only one instance of ImageCutWrapper for Nancy module. We need follow 3 steps:
 
 * Create `ImageCutWrapper` in module constructor, fill its all necessary properties and store object reference into module field. 
 * Call `Init` method on ImageCutWrapper instance. Among other things it extends module by methods handling PictureCut specific requests
 * Store reference to `ImageCutWrapper` in ViewBag. For example `ViewBag["pictureCut"] = _pictureCutWrapper`. This reference will be used in view for rendering some html and javascript code.  
 
 
 First two steps must be done in module constructor. Last step must be done in all http request handling methods provided by your module that results views with PictureCut functionality.
 
#### Create view

Finally you need view to present user interface. Three steps must be done:

* jQuery UI reference
* PictureCut javascript reference
* graphic container for dropping images

##### jQueryUI

PictureCut requirements are described [here](http://picturecut.tuyoshi.com.br/docs/#Installation). For `Nancy.PictureCut` only client side items jQuery, jQuery UI and BootStrap needs to be included in project. Assume your application layout already contains many of necessary files (as described above) you need include just one additional javascript `jQuery UI` in your view.  

```
 <script src="~/Content/jquery-ui.min.js"></script>
```

##### PictureCut javascript

You need to include PictureCut engine (jquery.picture.cut.js) and put some configuration javascript code. Fortunatelly `ImageCutWrapper` can do this for you. Just call two methods.

```
@pictureCut.RenderJsLinks(Html.RenderContext.Context)
@pictureCut.RenderJsCode()
```

As the result you get something like this

```
<script language="javascript" src="/ImageCut/jquery.picture.cut.js"></script>
<script type="text/javascript">
$('#container_image').PictureCut({
    InputOfImageDirectory: 'image3e66dac436b04f00bfd9c5c589996353',
    PluginFolderOnServer: '/',
    FolderOnServer: '/Demo/MyTemporaryImages/',
    EnableCrop: true,
    CropWindowStyle: 'Bootstrap',
    ActionToSubmitUpload: '/Demo/Photo/ImageCutter/',
    ActionToSubmitCrop: '/Demo/Photo/ImageCutter/',
    CropOrientation: false,
    UploadedCallback: function(data){ alert('This is custom javascript code');},
    CropModes: {widescreen: true, letterbox: false, free: true, square: true}
});
</script>
```

pictureCut reference on view side can be obtained by code: `PictureCutWrapper pictureCut = ViewBag.pictureCut;`.

#####Graphic container
This can be done simply by calling `RenderContainer` method. Consider following code:
```
<div class="row">
    <div class="col-lg-8">
        @pictureCut.RenderContainer()
    </div>
</div>
```
