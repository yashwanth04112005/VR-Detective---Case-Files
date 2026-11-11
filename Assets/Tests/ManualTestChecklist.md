# VR Detective: Case Files — Manual Test Checklist

Use this checklist during a playthrough. Mark **Pass** or **Fail** and note any issue beside the case. These are manual gameplay tests, not automated Unity Test Runner tests.

| ID | Test case | Steps | Expected result | Result / notes |
|---|---|---|---|---|
| T01 | Start game | From the Main Menu, select Start. | The Case Room opens. | ☐ Pass ☐ Fail — |
| T02 | Scene order | Complete each scene and use its prompted transition. | Flow is Main Menu → Case Room → Research Archive → Detective Board → Case Conclusion. | ☐ Pass ☐ Fail — |
| T03 | Player movement | Move and look around in the Case Room and Research Archive. | Walking and camera look respond correctly in both scenes. | ☐ Pass ☐ Fail — |
| T04 | Sprint | Hold Shift while moving, then release it. | Movement becomes faster while held and returns to normal afterward. | ☐ Pass ☐ Fail — |
| T05 | Crouch | Hold Ctrl, move, then release Ctrl. | Player crouches while held and returns to standing afterward. | ☐ Pass ☐ Fail — |
| T06 | Jump | Press Space while grounded. | Player jumps; jumping is unavailable while crouching. | ☐ Pass ☐ Fail — |
| T07 | Interaction range | Aim at a clue from nearby and press E; repeat from beyond interaction range. | Nearby clue can be examined; distant clue cannot. | ☐ Pass ☐ Fail — |
| T08 | Duplicate interaction | Examine the same Case Room clue twice with E. | Clue details can reopen, but the evidence count increases only once. | ☐ Pass ☐ Fail — |
| T09 | Case Room evidence | Examine each required Case Room clue. | Each clue is recorded once and updates evidence progress. | ☐ Pass ☐ Fail — |
| T10 | USB unlock | Collect the four required Case Room clues. | The USB is revealed or made available. | ☐ Pass ☐ Fail — |
| T11 | USB visibility and interaction | Locate the USB on the vending machine and examine it with E. | USB is visible at its intended location and can be examined. | ☐ Pass ☐ Fail — |
| T12 | USB evidence registration | Examine the USB, then examine it again. | USB details are recorded once; revisiting it does not increase the count again. | ☐ Pass ☐ Fail — |
| T13 | Case Room transition | Collect the required Case Room evidence and press F when prompted. | The Research Archive opens. | ☐ Pass ☐ Fail — |
| T14 | Archive clue collection | Examine each of the three Archive records, including one twice. | All three are recorded; repeating one does not add another record. | ☐ Pass ☐ Fail — |
| T15 | Archive transition | Complete the Archive investigation and press F when prompted. | The Detective Board opens. | ☐ Pass ☐ Fail — |
| T16 | Evidence persistence | Collect evidence and progress through the scene transitions. | Case Room, USB, and Archive evidence remains available in later scenes. | ☐ Pass ☐ Fail — |
| T17 | Board evidence display | Open the Detective Board and select several evidence cards. | Collected evidence appears with the correct titles/details and related clues are connected. | ☐ Pass ☐ Fail — |
| T18 | Board navigation | Select Continue on the Detective Board. | Case Conclusion opens. | ☐ Pass ☐ Fail — |
| T19 | Quiz scoring | Answer several quiz questions correctly and incorrectly, submitting each. | Correct answers increase the score; incorrect answers do not. Feedback explains the evidence reasoning. | ☐ Pass ☐ Fail — |
| T20 | Final result | Complete all four quiz questions with a known mix of right and wrong answers. | Final score, rating, and verdict match the submitted answers. | ☐ Pass ☐ Fail — |

## Run details

- Tester: ____________________
- Date: ____________________
- Unity version / build: ____________________
- Overall notes: __________________________________________________________
