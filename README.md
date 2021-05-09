# LiveCharacter

A Live2D Character Animation System Developed Under Unity Framework

## 前言

此系统（软件）用于硕士毕业设计

系统功能旨在将AI与CV及CG结合，快速提取视频中的人体运动并应用于二维角色，生成合理流畅的二维角色动画

原打算用C++开发，但限于时间及各种原因，还是决定用Unity下的C#来快速开发一个Demo出来

此系统将由[Oxine](https://github.com/oxine)和[Keneyr](https://github.com/Keneyr)协作完成

由于文件大小限制，以下两个路径下的文件没有上传(读者请于[OpenPose官方github](https://github.com/CMU-Perceptual-Computing-Lab/openpose_unity_plugin)处下载)：

LiveCharacter\Truck\Assets\StreamingAssets\models
![models](Tutorials/models.png)

LiveCharacter\Truck\Assets\OpenPose\Plugins
![dlls](Tutorials/dlls.png)

## 开发环境

Win10/Unity2018.3.3f1

## 预备知识

OpenPose人体姿态识别

BP神经网络分类

轮廓检测

网格剖分

骨骼蒙皮

姿态检索

运动对齐

## 原型设计

Game视图下像素分辨率：1024 * 768

Canvas下的Button/Image等组件作为渲染桥梁

1、导入视频并渲染到Video框下，同时运行OpenPose读取视频帧数及检测骨骼存取帧数据到本地

方法：

参考该[博客](https://gameinstitute.qq.com/community/detail/125314),在run模式下，点击按钮，打开本地文件夹，选择特定视频路径

内部即刻运行OpenPose算法，对视频数据每帧检测，保存渲染好的视频帧图像到本地，根据用户选择的初始帧及帧间隔进行可视化(方便用户交互快速反应，实时计算大概率延迟较高)

使用Unity自带的Video Player将视频原始数据渲染至Canvas上（由于Unity自带的组件不好用，所以改为AVProVideo插件，参考该[博客教程](https://www.cnblogs.com/mrmocha/p/8087389.html)）

2、导入角色图像并显示

类似Video导入，原像素进行算法处理，可视化适当缩放

3、实时控制台输出--Debug.Log()到Editor编辑器，同时也实时滚动输出到系统界面



## 系统使用说明


## 参考文献

Cao Z, Simon T, Wei S E, et al. Realtime multi-person 2d pose estimation using part affinity 
fields[C]//Proceedings of the IEEE conference on computer vision and pattern recognition. 2017: 
7291-7299.

Li, Jing, et al. "Brief introduction of back propagation (BP) neural network algorithm and its 
improvement." Advances in computer science and information engineering. Springer, Berlin, 
Heidelberg, 2012. 553-558.

Suzuki S. Topological structural analysis of digitized binary images by border following[J]. 
Computer vision, graphics, and image processing, 1985, 30(1): 32-46.

Douglas D H, Peucker T K. Algorithms for the reduction of the number of points required to represent 
a digitized line or its caricature[J]. Cartographica: the international journal for geographic information
and geovisualization, 1973, 10(2): 112-122.

Dwyer, Rex A. "A faster divide-and-conquer algorithm for constructing Delaunay 
triangulations." Algorithmica 2.1 (1987): 137-151.

Jacobson, Alec, et al. "Bounded biharmonic weights for real-time deformation." ACM Trans. 
Graph. 30.4 (2011): 78.


