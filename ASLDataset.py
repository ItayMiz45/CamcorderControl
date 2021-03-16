import torch
import os
import pandas as pd
from torch.utils.data import Dataset
import cv2


class ASLDataset(Dataset):
    def __init__(self, csv_file, root_dir, transform=None):
        self.annotations = pd.read_csv(fr"{root_dir}\..\{csv_file}")
        self.root_dir = root_dir
        self.transform = transform

    def __len__(self):
        return len(self.annotations)

    def __getitem__(self, index):
        img_path = os.path.join(self.root_dir, self.annotations.iloc[index, 0])
        image = cv2.imread(img_path)
        ann = self.annotations.iloc[index, 1]
        if ann.isdigit():
            y_label = torch.tensor(int(ann))
        else:
            y_label = torch.tensor(ord(ann)-ord('a')+1)

        if self.transform:
            image = self.transform(image)

        return image, y_label
