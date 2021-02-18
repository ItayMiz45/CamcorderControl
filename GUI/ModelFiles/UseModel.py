from HandSegmenter import HandSegmenter
from CNN_Gest import Net
import cv2
import torch
from torchvision import transforms
import time
import struct
import numpy as np
import io
from PIL import Image

MODEL_PATH = './custom_model_gest_BACK.pth'

IS_MASKED = False
IS_PIPE = True
CONFIDENCE_THRESH = 5
CORRECT_IN_A_ROW = 20
MISTAKES = 2


transform = transforms.Compose(
    [
        transforms.ToTensor(),
        transforms.Resize(32),
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

    hs = HandSegmenter(mask_hand=IS_MASKED)

    net = Net()
    net.to(device)

    net.load_state_dict(torch.load(MODEL_PATH, map_location=torch.device('cpu')))

    pipe_path = "NUL"

    if IS_PIPE:
        pipe_path = r'\\.\pipe\CamcorderPipe'

    with open(pipe_path, 'r+b', 0) as f:
        wrong_cnt = 0
        last = -1

        for i, (org, proc) in enumerate(hs):
            #cv2.imshow("original", org)
            #cv2.imshow("processed", proc)

            if not IS_MASKED:
                proc = cv2.cvtColor(proc, cv2.COLOR_GRAY2BGR)

            proc = transform(proc)

            outputs = net(proc[None, ...])
            _, predicted = torch.max(outputs, 1)

            # print(predicted.item())

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
                    print(predicted.item())
                else:
                    print(-1)

                wrong_cnt = 0

            # print("\n\n")

            if IS_PIPE:
                # send to pipe
                try:
                    img_bytes = ndarray_to_formatted_img(org)
                    to_send = struct.pack('I', len(img_bytes)) + img_bytes
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

            # time.sleep(1 / 30)

            if cv2.waitKey(1) & 0xFF == ord('q'):
                break

    cv2.destroyAllWindows()


if __name__ == '__main__':
    main()
