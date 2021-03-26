from HandSegmenter import HandSegmenter
import cv2
import torch
from torchvision import transforms
import struct
import numpy as np
import io
from PIL import Image
from CNN_Gest import Net as Net_Gest
from CNN_Side import Net as Net_Side
import os


MODEL_GEST_PATH = './gest.pth'
MODEL_SIDES_PATH = './sides.pth'


IS_PIPE = True
CONFIDENCE_THRESH = 5
CORRECT_IN_A_ROW = 45
MISTAKES = 2


transform_gest = transforms.Compose(
    [
        transforms.ToTensor(),
        transforms.Resize(32),
        transforms.Normalize((0.5, 0.5, 0.5), (0.5, 0.5, 0.5))
    ]
)

transform_side = transforms.Compose(
    [
        transforms.ToTensor(),
        transforms.Resize(100),
        transforms.Normalize((0.5, 0.5, 0.5), (0.5, 0.5, 0.5))
    ]
)


def ndarray_to_formatted_img(arr: np.ndarray, img_format="PNG") -> bytes:
    img = Image.fromarray(arr)
    with io.BytesIO() as output:
        img.save(output, format=img_format)
        return output.getvalue()


def main():
    # device = torch.device("cuda:0" if torch.cuda.is_available() else "cpu")
    device = "cpu"

    hs = HandSegmenter(both=True)

    net_gest = Net_Gest()
    net_side = Net_Side()

    net_gest.to(device)
    net_side.to(device)

    net_gest.load_state_dict(torch.load(MODEL_GEST_PATH, map_location=torch.device(device)))
    net_side.load_state_dict(torch.load(MODEL_SIDES_PATH, map_location=torch.device(device)))

    pipe_path = "NUL"

    if IS_PIPE:
        pipe_path = r'\\.\pipe\CamcorderPipe'

    exec_command: bool

    with open(pipe_path, 'r+b', 0) as f:
        wrong_cnt_gest = 0
        last_gest = -1

        wrong_cnt_side = 0
        last_side = -1

        for i, (org, filled, masked) in enumerate(hs):
            exec_command = False
            filled = cv2.cvtColor(filled, cv2.COLOR_GRAY2BGR)
            filled = transform_gest(filled)
            masked = transform_side(masked)

            outputs_gest = net_gest(filled[None, ...])
            _, predicted_gest = torch.max(outputs_gest, 1)

            outputs_side = net_side(masked[None, ...])
            _, predicted_side = torch.max(outputs_side, 1)

            if torch.topk(outputs_gest, 1)[0] > CONFIDENCE_THRESH:
                if last_gest != predicted_gest.item():
                    wrong_cnt_gest += 1

                if wrong_cnt_gest > MISTAKES:
                    last_gest = predicted_gest.item()
                    wrong_cnt_gest = 0

            else:
                wrong_cnt_gest += 1

            if i % CORRECT_IN_A_ROW == 0:  # check every few frames
                print('\n')

                if wrong_cnt_gest < MISTAKES:
                    print("Gesture:", predicted_gest.item())
                    exec_command = True
                else:
                    print("Gesture:", -1)

                wrong_cnt_gest = 0


            if torch.topk(outputs_side, 1)[0] > CONFIDENCE_THRESH:
                if last_side != predicted_side.item():
                    wrong_cnt_side += 1

                if wrong_cnt_side > MISTAKES:
                    last_side = predicted_side.item()
                    wrong_cnt_side = 0

            else:
                wrong_cnt_side += 1

            if i % CORRECT_IN_A_ROW == 0:  # check every few frames
                if wrong_cnt_side < MISTAKES:
                    if predicted_side.item() == 0:
                        exec_command = False
                        print("sideure:", -1)

                    else:
                        print("sideure:", predicted_side.item())
                        exec_command = True
                else:
                    exec_command = False
                    print("sideure:", -1)

                wrong_cnt_side = 0

            if IS_PIPE:
                # send to pipe
                try:
                    """
                    Send:
                    <ImageLength><ImageData><ExecuteCommand: Bool><Gesture ID><Side>
                    """
                    img_bytes = ndarray_to_formatted_img(org)
                    to_send = struct.pack('I', len(img_bytes)) + img_bytes
                    to_send += struct.pack('I', 1) if exec_command else struct.pack('I', 0)
                    to_send += struct.pack('I', predicted_gest.item())
                    to_send += struct.pack('I', predicted_side.item())
                    f.write(to_send)  # Write image length and image
                    f.seek(0)  # EDIT: This is also necessary

                except Exception as e:
                    print(len(img_bytes))
                    print(f"Error: {e}")
                    break


if __name__ == '__main__':
    main()
