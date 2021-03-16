import cv2
import os
from tkinter import messagebox as mb
import threading
from HandSegmenter import HandSegmenter

IS_MASKED = False
VIDEO_PATH = "output.avi"
TMP_VIDEO_PATH = "tmp.avi"


def clean_vids():
    if os.path.exists(TMP_VIDEO_PATH) and os.path.exists(VIDEO_PATH):
        os.remove(VIDEO_PATH)  # last video
        os.rename(TMP_VIDEO_PATH, VIDEO_PATH)


def is_cap_color(cap: cv2.VideoCapture, set_to=0):
    res, frame = cap.read()
    cap.set(cv2.CAP_PROP_POS_FRAMES, 0)
    return frame.shape[2] == 3


def copy_video():
    cap = cv2.VideoCapture(VIDEO_PATH)
    out = cv2.VideoWriter(TMP_VIDEO_PATH, cv2.VideoWriter_fourcc(*"DIVX"), 20, (200, 200), IS_MASKED)

    while cap.isOpened():
        good_read, frame = cap.read()
        if not good_read:
            break

        if not IS_MASKED:
            gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
            thresh, frame = cv2.threshold(gray, 127, 255, cv2.THRESH_BINARY)

        out.write(frame)

    return out


start_capture = False


def get_user_input():
    global start_capture

    mb.showinfo("Start Capturing", "Press OK to start capturing video")

    start_capture = True


def main():
    if os.path.exists(VIDEO_PATH):
        print("Copying video...")
        out = copy_video()
        print("Done")
    else:
        out = cv2.VideoWriter(VIDEO_PATH, cv2.VideoWriter_fourcc(*"DIVX"), 20, (200, 200), IS_MASKED)

    hs = HandSegmenter(mask_hand=IS_MASKED)

    input_thread = threading.Thread(target=get_user_input)
    input_thread.start()

    for org, bin in hs:
        cv2.imshow("original", org)
        cv2.imshow("binary", bin)

        if start_capture:
            out.write(bin)

        if cv2.waitKey(1) & 0xFF == ord('q'):
            break

    out.release()
    cv2.destroyAllWindows()
    input_thread.join()
    clean_vids()


if __name__ == '__main__':
    main()
