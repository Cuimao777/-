   M S B A M L   `   `   `  ����  DesktopOrganizer"    DesktopOrganizer.InputDialog   5   	   W SPresentationCore, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      5   	   R NSystem.Xaml, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	     � Title$  输入对话框��5   	   $	��150��6   $	��300��6$    � WindowStartupLocation$ CenterOwner=�60    � 
ResizeMode$ NoResize=�6T   .��5      � $��10q�6    � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5	      $��* �6   .+�5   
   �� -   5         ��Name$ MessageText��5      $��请输入：��6)   $��0,0,0,5q�65   �� 5   
   -   5         ��Name$ ResponseTextBox��5      $��1��6+   $��0,0,0,10q�68   �� 5   
   $��2��6    �� Orientation$ 
Horizontal=�6"   $��Right=�6;   .+�5      �� -   5        $��确定��5      $��60��6"   $��0,0,10,0q�6D   �� 5      -   5        $��取消��5      $��60��6"   6-   5      5      5      