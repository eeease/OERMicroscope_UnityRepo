var UploadFilePlugin = {
  UploadFileJsLib: function(gameObjectName, methodName, fileExtension) {
    var gameObject = UTF8ToString(gameObjectName);
    var method = UTF8ToString(methodName);
    var format = UTF8ToString(fileExtension);
    var unitycanvas = document.getElementById('unity-canvas');
    if(!document.getElementById('UploadFileInput'))
    {
      var fileInput = document.createElement('input');
      fileInput.setAttribute('type', 'file');
      fileInput.setAttribute('id', 'UploadFileInput');
      fileInput.setAttribute('accept', '.'+format);
      fileInput.style.visibility = 'hidden';
      fileInput.style.display = 'none';
      fileInput.onclick = function (event)
      {
        this.value = null;
        var element = document.getElementById('UploadFileInput');
        element.parentNode.removeChild(element);
	unitycanvas.removeEventListener('click', OpenFileDialog, false);
      };
      fileInput.onchange = function (event)
      {
        if(event.target.value != null)
        {
          var fn = event.target.files[0].name;
          var ext = fn.substring(fn.lastIndexOf('.')+1, fn.length)
          console.log('Filename: '+event.target.files[0].name+' / Extension: '+ext);
	  if(ext == format) SendMessage(gameObject, method, URL.createObjectURL(event.target.files[0]));
          else console.log('File extension not allowed: '+ext);
        }
      };
      document.body.appendChild(fileInput);
    }
    var OpenFileDialog = function()
    {
      document.getElementById('UploadFileInput').click();
    };
    unitycanvas.addEventListener('click', OpenFileDialog, false);
  },
  UploadTextureJsLib: function(gameObjectName, methodName, maxSize, imageFormat) {
    var gameObject = UTF8ToString(gameObjectName);
    var method = UTF8ToString(methodName);
    var format = UTF8ToString(imageFormat);
    var unitycanvas = document.getElementById('unity-canvas');
    if(!document.getElementById('UploadTextureInput'))
    {
      var fileInput = document.createElement('input');
      fileInput.setAttribute('type', 'file');
      fileInput.setAttribute('id', 'UploadTextureInput');
      if(format == "png")
      {
        fileInput.setAttribute('accept', '.png');
      }
      else
      {
        fileInput.setAttribute('accept', '.jpg, .jpeg, .png');
      }
      fileInput.style.visibility = 'hidden';
      fileInput.style.display = 'none';
      fileInput.onclick = function (event)
      {
        this.value = null;
        var element = document.getElementById('UploadTextureInput');
        element.parentNode.removeChild(element);
	unitycanvas.removeEventListener('click', OpenFileDialog, false);
      };
      fileInput.onchange = function (event)
      {
        if(event.target.value != null)
        {
          var fn = event.target.files[0].name;
          var ext = fn.substring(fn.lastIndexOf('.')+1, fn.length)
          console.log('Filename: '+event.target.files[0].name+' / Extension: '+ext);
	  if((format == "png" && ext == format) || (format == "jpg" && (ext == format || ext == "jpeg" || ext == "png")))
          {
            format = ext;
            if(maxSize > 0) resize_image(event.target.files[0])
            else SendMessage(gameObject, method, URL.createObjectURL(event.target.files[0]));
          }
          else console.log('File extension not allowed: '+ext);
        }
      };
      document.body.appendChild(fileInput);
    }
    var OpenFileDialog = function()
    {
      document.getElementById('UploadTextureInput').click();
    };
    unitycanvas.addEventListener('click', OpenFileDialog, false);

    //resize image function
    function resize_image(file)
    {
      var reader = new FileReader();
      reader.onloadend = function()
      {
        var tempImg = new Image();
        tempImg.src = reader.result;
        tempImg.onload = function()
        {
          var tempW = tempImg.width;
          var tempH = tempImg.height;
          if (tempW > tempH)
          {
            if (tempW > maxSize)
            {
              tempH *= maxSize / tempW;
              tempW = maxSize;
            }
          }
          else
          {
            if (tempH > maxSize)
            {
              tempW *= maxSize / tempH;
              tempH = maxSize;
            }
          }
          var dataURL = "";
          var newBLOB = null;
          try
          {
            if(document.getElementById('img-canvas')) document.getElementById('img-canvas').remove();
            var canvas = document.createElement('canvas');
            canvas.setAttribute('id', 'img-canvas');
            canvas.width = tempW;
            canvas.height = tempH;
            var ctx = canvas.getContext("2d");
            ctx.drawImage(this, 0, 0, tempW, tempH);
            if(format == "png") dataURL = canvas.toDataURL("image/png");
            else dataURL = canvas.toDataURL("image/jpeg");
            function dataURItoBlob(dataURI)
            {
              var mime = dataURI.split(',')[0].split(':')[1].split(';')[0];
              var binary = atob(dataURI.split(',')[1]);
              var array = [];
              for (var i = 0; i < binary.length; i++)
              {
                array.push(binary.charCodeAt(i));
              }
              return new Blob([new Uint8Array(array)], {type: mime});
            }
            newBLOB = ((window.URL || window.webkitURL) || URL).createObjectURL(dataURItoBlob(dataURL));
          }
          catch(err)
          {
            alert('Error: ' + err.message);
          }
          finally
          {
            if(newBLOB === null) alert('Error: Empty URL...');
            SendMessage(gameObject, method, newBLOB);
          }
        }//end tempImg.onload
      }//end reader.onloadend
      reader.readAsDataURL(file);
    }//end resize_image
  }//end UploadTexture
};
mergeInto(LibraryManager.library, UploadFilePlugin);