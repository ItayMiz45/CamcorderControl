from typing import Tuple, Union

import cv2
import numpy as np


class HandSegmenter:

    def __init__(self, dilate_iter=6, mask_hand=False, both=False):
        self._cap = cv2.VideoCapture(0)
        self._fgbg2 = cv2.createBackgroundSubtractorMOG2()
        self._shape = 200, 200
        self._dilate_iter = dilate_iter
        self._mask_hand = mask_hand
        self._both = both  # masked and unmasked

    def __iter__(self):
        return self

    def __next__(self) -> Union[Tuple[np.ndarray, np.ndarray], Tuple[np.ndarray, np.ndarray, np.ndarray]]:
        while True:
            ret, frame = self._cap.read()
            frame = cv2.flip(frame, 1)

            if not ret:
                break

            # define region of interest
            roi = frame[100:300, 100:300]

            fgmask2 = self._fgbg2.apply(roi)

            kernel = np.ones((3, 3))

            dilated = cv2.dilate(fgmask2, kernel, iterations=self._dilate_iter)
            blurred = cv2.GaussianBlur(dilated, (5, 5), 0)

            cv2.rectangle(frame, (100, 100), (300, 300), (0, 255, 0), 0)

            ret, thresh2 = cv2.threshold(blurred, 120, 255, cv2.THRESH_BINARY)

            im_shape = thresh2.shape

            filled = thresh2.copy()
            cv2.line(filled, (0, im_shape[1]), (im_shape[0], im_shape[1]), (255, 255, 255), 25)
            contours, hierarchy = cv2.findContours(filled, cv2.RETR_TREE, cv2.CHAIN_APPROX_SIMPLE)

            if len(contours) == 0:
                continue

            cnt = [max(contours, key=cv2.contourArea)]
            cv2.drawContours(filled, cnt, -1, (255, 255, 255), cv2.FILLED)

            cv2.line(filled, (0, im_shape[1]), (im_shape[0], im_shape[1]), (0, 0, 0), 25)

            if self._both:
                masked = cv2.bitwise_and(frame[100:300, 100:300], frame[100:300, 100:300], mask=filled)
                return frame, filled, masked

            if self._mask_hand:
                filled = cv2.bitwise_and(frame[100:300, 100:300], frame[100:300, 100:300], mask=filled)

            return frame, filled

    def __del__(self):
        self._cap.release()
        print("Done")

    def get_size(self):
        return 200, 200

    def get_cap_size(self):
        return int(self._cap.get(3)), int(self._cap.get(4))


if __name__ == '__main__':
    hs = HandSegmenter(both=True)

    for frame, filled, masked in hs:
        cv2.imshow("Frame", frame)
        cv2.imshow("filled", filled)
        cv2.imshow("masked", masked)

        if cv2.waitKey(1) & 0xFF == ord('q'):
            break

    cv2.destroyAllWindows()
