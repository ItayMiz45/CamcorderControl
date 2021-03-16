from HandSegmenter import HandSegmenter
import cv2
import torch
from torchvision import transforms
import struct
import numpy as np
import io
from PIL import Image
import os


MODEL_PATH = './custom_model_gest_BACK.pth'

if 'asl' in MODEL_PATH:
    from CNN_ASL import Net
    IS_MASKED = False
    parse_pred = lambda x: x if x == 0 else chr(x + ord('a') - 1)

elif 'gest' in MODEL_PATH:
    from CNN_Gest import Net
    IS_MASKED = False
    parse_pred = lambda x: x

elif 'side' in MODEL_PATH:
    from CNN_Side import Net
    IS_MASKED = True
    parse_pred = lambda x: x

else:
    exit(1)


IS_PIPE = False
CONFIDENCE_THRESH = 5
CORRECT_IN_A_ROW = 45
MISTAKES = 2


transform = transforms.Compose(
    [
        transforms.ToTensor(),
        transforms.Resize(32),
        transforms.Normalize((0.5, 0.5, 0.5), (0.5, 0.5, 0.5))
    ]
)


def do_action(gest: int):
    print(gest)

    # if gest == -1 or gest == 0:
    #     return  # no gest / not sure
    #
    # if gest == 1:
    #     os.system("ping 8.8.8.8")
    #
    # if gest == 5:
    #     os.system("explorer")


def ndarray_to_formatted_img(arr: np.ndarray, img_format="PNG") -> bytes:
    img = Image.fromarray(arr)
    with io.BytesIO() as output:
        img.save(output, format=img_format)
        return output.getvalue()


def main():
    # device = torch.device("cuda:0" if torch.cuda.is_available() else "cpu")
    device = "cpu"

    hs = HandSegmenter(mask_hand=IS_MASKED)

    net = Net()
    net.to(device)

    net.load_state_dict(torch.load(MODEL_PATH, map_location=torch.device('cpu')))

    pipe_path = "NUL"

    if IS_PIPE:
        pipe_path = r'\\.\pipe\CamcorderPipe'

    exec_command: bool

    with open(pipe_path, 'r+b', 0) as f:
        wrong_cnt = 0
        last = -1

        for i, (org, proc) in enumerate(hs):
            exec_command = False
            cv2.imshow("original", org)
            cv2.imshow("processed", proc)

            if not IS_MASKED:
                proc = cv2.cvtColor(proc, cv2.COLOR_GRAY2BGR)

            proc = transform(proc)

            outputs = net(proc[None, ...])
            _, predicted = torch.max(outputs, 1)

            if torch.topk(outputs, 1)[0] > CONFIDENCE_THRESH:
                if last != predicted.item():
                    wrong_cnt += 1

                if wrong_cnt > MISTAKES:
                    last = predicted.item()
                    wrong_cnt = 0

            else:
                wrong_cnt += 1

            if i % CORRECT_IN_A_ROW == 0:  # check every few frames
                if wrong_cnt < MISTAKES:
                    do_action(predicted.item())
                    exec_command = True
                else:
                    print(-1)

                wrong_cnt = 0

            if IS_PIPE:
                # send to pipe
                try:
                    """
                    Send:
                    <ImageLength><ImageData><ExecuteCommand: Bool><Gesture ID>
                    """
                    img_bytes = ndarray_to_formatted_img(org)
                    to_send = struct.pack('I', len(img_bytes)) + img_bytes
                    to_send += struct.pack('I', 1) if exec_command else struct.pack('I', 0)
                    to_send += struct.pack('I', predicted.item())
                    f.write(to_send)  # Write image length and image
                    f.seek(0)  # EDIT: This is also necessary

                    n = struct.unpack('I', f.read(4))[0]  # Read str length
                    s2 = f.read(n)  # Read str
                    f.seek(0)  # Important!!!
                    print('Read:', s2)

                except Exception as e:
                    print(len(img_bytes))
                    print(f"Error: {e}")
                    break

            if cv2.waitKey(1) & 0xFF == ord('q'):
                break

    cv2.destroyAllWindows()


if __name__ == '__main__':
    main()
