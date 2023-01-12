# Wuolan't
<img src="https://user-images.githubusercontent.com/35693714/207497352-ec236635-e6f4-4bf2-a143-452311299edd.png" width="100"><br>
[🇬🇧] A program that removes ads from Wuolah-generated PDFs.<br>
[🇪🇸] Un programa que elimina los anuncios de PDFs generados en Wuolah.<br>
<b>[Download here (CLI + Windows GUI)](https://github.com/KaruzoHikari/Wuolan-t/releases/latest)</b><br>

## Features
Are you tired of having dozens of ads in your Wuolah PDFs, when you're just trying to study for the exam?<br>
Just drag and drop your Wuolah files into the program, and they'll instantly be distraction-free!<br>
A new folder called "Wuolan't" will be automatically created, where your ad-less PDFs will be placed.

You can also choose the page size:
- Original will keep each image's original size (recommended).
- A4 will embed the images into a standard A4 page size.

<img src="https://user-images.githubusercontent.com/35693714/208251643-906b238b-358e-4ddc-8405-0e7b42d5a1b9.png" width="600"><br>

## CLI usage
You can use the app from a commandline. Just follow the usage:<br>
- `wuolant.exe [page size: original/A4] [pdf paths]`<br>

If you skip the page size, it'll default to original page size.

### Examples:
- `wuolant.exe original "C:\Users\Admin\Documents\test.pdf"`<br>
- `wuolant.exe a4 "C:\Users\Admin\Documents\mypdf1.pdf" "C:\Users\Admin\Documents\mypdf2.pdf"`
