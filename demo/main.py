import tkinter as tk
from tkinter import ttk, messagebox
from tkinter.filedialog import askdirectory
import os
from pathlib import Path
from typing import List

class DropZone:
    def __init__(self, name: str, path: str):
        self.name = name
        self.path = path
        self._file_count = len([f for f in os.listdir(path) if os.path.isfile(os.path.join(path, f))])
    
    @property
    def file_count(self) -> int:
        return self._file_count
    
    @file_count.setter
    def file_count(self, value: int):
        self._file_count = value

class InputDialog(tk.Toplevel):
    def __init__(self, parent, title: str, message: str, default_value: str = ""):
        super().__init__(parent)
        self.title(title)
        self.result = None
        
        tk.Label(self, text=message).pack(padx=10, pady=5)
        
        self.entry = tk.Entry(self)
        self.entry.insert(0, default_value)
        self.entry.pack(padx=10, pady=5)
        
        btn_frame = tk.Frame(self)
        btn_frame.pack(pady=10)
        
        tk.Button(btn_frame, text="确定", command=self.ok_click).pack(side=tk.LEFT, padx=5)
        tk.Button(btn_frame, text="取消", command=self.cancel_click).pack(side=tk.LEFT, padx=5)
        
        self.transient(parent)
        self.grab_set()
        
    def ok_click(self):
        self.result = self.entry.get()
        self.destroy()
        
    def cancel_click(self):
        self.destroy()

class MainWindow(tk.Tk):
    def __init__(self):
        super().__init__()
        
        self.title("桌面整理助手")
        self.attributes('-alpha', 0.9)  # 初始透明度
        self.overrideredirect(True)  # 无边框窗口
        
        # 创建默认存储目录
        base_dir = Path.home() / "Documents" / "OrganizedFiles"
        base_dir.mkdir(parents=True, exist_ok=True)
        
        self.drop_zones: List[DropZone] = []
        
        # 初始化UI
        self._init_ui()
        self._init_drop_zones(base_dir)
        
        # 绑定窗口拖动
        self.title_bar.bind('<Button-1>', self.start_move)
        self.title_bar.bind('<B1-Motion>', self.do_move)
        
    def _init_ui(self):
        # 主布局
        main_frame = tk.Frame(self, bg='white')
        main_frame.pack(fill=tk.BOTH, expand=True)
        
        # 标题栏
        self.title_bar = tk.Frame(main_frame, bg='#f0f0f0', height=30)
        self.title_bar.pack(fill=tk.X)
        tk.Label(self.title_bar, text="桌面整理助手", bg='#f0f0f0').pack(side=tk.LEFT, padx=10)
        
        # 透明度控制
        opacity_frame = tk.Frame(main_frame, bg='white')
        opacity_frame.pack(fill=tk.X, padx=10, pady=5)
        tk.Label(opacity_frame, text="透明度：", bg='white').pack(side=tk.LEFT)
        opacity_scale = ttk.Scale(opacity_frame, from_=0.1, to=1.0, value=0.9,
                                 orient=tk.HORIZONTAL, command=self.on_opacity_change)
        opacity_scale.pack(side=tk.LEFT, fill=tk.X, expand=True)
        
        # 添加新区域按钮
        tk.Button(main_frame, text="添加新区域", command=self.add_new_zone).pack(pady=5)
        
        # 放置区域容器
        self.zones_frame = tk.Frame(main_frame, bg='white')
        self.zones_frame.pack(fill=tk.BOTH, expand=True, padx=10, pady=5)
        
    def _init_drop_zones(self, base_dir: Path):
        # 初始化默认区域
        for i in range(1, 5):
            self.add_drop_zone(f"区域{i}", "")
    
    def add_drop_zone(self, name: str, path: str):
        if len(self.drop_zones) >= 8:
            return
            
        drop_zone = DropZone(name, path)
        self.drop_zones.append(drop_zone)
        
        # 创建拖放区域UI
        zone_frame = tk.Frame(self.zones_frame, relief=tk.GROOVE, bd=2)
        zone_frame.pack(fill=tk.X, pady=5)
        
        name_label = tk.Label(zone_frame, text=f"{name} ({drop_zone.file_count}个文件)")
        name_label.pack(side=tk.LEFT, padx=5)
        
        tk.Button(zone_frame, text="修改名称", 
                  command=lambda z=drop_zone, l=name_label: self.edit_zone_name(z, l)).pack(side=tk.RIGHT, padx=2)
        tk.Button(zone_frame, text="自定义路径",
                  command=lambda z=drop_zone, l=name_label: self.customize_path(z, l)).pack(side=tk.RIGHT, padx=2)
        
        # 设置拖放功能
        zone_frame.drop_target_register("*")
        zone_frame.dnd_bind('<<Drop>>', lambda e, z=drop_zone, l=name_label: self.on_drop(e, z, l))
    
    def edit_zone_name(self, zone: DropZone, label: tk.Label):
        dialog = InputDialog(self, "修改名称", "请输入新的名称：", zone.name)
        self.wait_window(dialog)
        if dialog.result:
            zone.name = dialog.result
            label.config(text=f"{zone.name} ({zone.file_count}个文件)")
    
    def customize_path(self, zone: DropZone, label: tk.Label):
        new_path = askdirectory(initialdir=zone.path)
        if new_path:
            # 移动现有文件
            try:
                for file in os.listdir(zone.path):
                    old_file = os.path.join(zone.path, file)
                    new_file = os.path.join(new_path, file)
                    if os.path.isfile(old_file):
                        if os.path.exists(new_file):
                            if messagebox.askyesno("文件已存在", f"文件 {file} 已存在于目标文件夹中，是否覆盖？"):
                                os.replace(old_file, new_file)
                        else:
                            os.rename(old_file, new_file)
                
                zone.path = new_path
                zone._file_count = len([f for f in os.listdir(new_path) if os.path.isfile(os.path.join(new_path, f))])
                label.config(text=f"{zone.name} ({zone.file_count}个文件)")
            except Exception as e:
                messagebox.showerror("错误", f"移动文件时发生错误：{str(e)}")
    
    def on_drop(self, event, zone: DropZone, label: tk.Label):
        files = event.data.split('}')
        for file in files:
            file = file.strip('{').strip()
            if os.path.exists(file):
                try:
                    new_path = os.path.join(zone.path, os.path.basename(file))
                    os.rename(file, new_path)
                    zone._file_count += 1
                    label.config(text=f"{zone.name} ({zone.file_count}个文件)")
                except Exception as e:
                    messagebox.showerror("错误", f"移动文件失败：{str(e)}")
    
    def add_new_zone(self):
        if len(self.drop_zones) < 8:
            new_index = len(self.drop_zones) + 1
            new_path = os.path.join(os.path.expanduser("~"), "Documents", "OrganizedFiles", f"Area{new_index}")
            os.makedirs(new_path, exist_ok=True)
            self.add_drop_zone(f"区域{new_index}", new_path)
    
    def on_opacity_change(self, value):
        self.attributes('-alpha', float(value))
    
    def start_move(self, event):
        self._drag_data = {'x': event.x, 'y': event.y}
    
    def do_move(self, event):
        if hasattr(self, '_drag_data'):
            dx = event.x - self._drag_data['x']
            dy = event.y - self._drag_data['y']
            self.geometry(f"+{self.winfo_x() + dx}+{self.winfo_y() + dy}")

if __name__ == "__main__":
    app = MainWindow()
    app.mainloop()