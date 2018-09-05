import math, time
	
global prev_back, mode, offset, message_time

def sign(x): return 1 if x >= 0 else -1

# conjugate quaternion
def conj(q):
  return [-q[0], -q[1], -q[2], q[3]]

# multiplication of quaternion
def multiply(a, b):
  x0, y0, z0, w0 = a
  x1, y1, z1, w1 = b
#  return [x1 * w0 - y1 * z0 + z1 * y0 + w1 * x0,
#      x1 * z0 + y1 * w0 - z1 * x0 + w1 * y0,
#      -x1 * y0 + y1 * x0 + z1 * w0 + w1 * z0,
#      -x1 * x0 - y1 * y0 - z1 * z0 + w1 * w0]
  return [-x1 * y0 + y1 * x0 + z1 * w0 + w1 * z0,
      x1 * z0 + y1 * w0 - z1 * x0 + w1 * y0,
      x1 * w0 - y1 * z0 + z1 * y0 + w1 * x0,
      -x1 * x0 - y1 * y0 - z1 * z0 + w1 * w0]

# convert quaternion to euler
def quaternion2euler(q):
  yaw_pitch_roll = [0.0, 0.0, 0.0]
  # roll (x-axis rotation)
  sinr = +2.0 * (q[3] * q[0] + q[1] * q[2])
  cosr = +1.0 - 2.0 * (q[0] * q[0] + q[1] * q[1])
  yaw_pitch_roll[2] = math.atan2(sinr, cosr)

  # pitch (y-axis rotation)
  sinp = +2.0 * (q[3] * q[1] - q[2] * q[0])
  if (math.fabs(sinp) >= 1):
    yaw_pitch_roll[1] = math.copysign(math.pi / 2, sinp)
  else:
    yaw_pitch_roll[1] = math.asin(sinp)

  # yaw (z-axis rotation)
  siny = +2.0 * (q[3] * q[2] + q[0] * q[1]);
  cosy = +1.0 - 2.0 * (q[1] * q[1] + q[2] * q[2]);
  yaw_pitch_roll[0] = math.atan2(siny, cosy);

  return yaw_pitch_roll

# convert euler to quaternion
def euler2quaternion(yaw_pitch_roll):
  cy = math.cos(yaw_pitch_roll[0] * 0.5);
  sy = math.sin(yaw_pitch_roll[0] * 0.5);
  cr = math.cos(yaw_pitch_roll[2] * 0.5);
  sr = math.sin(yaw_pitch_roll[2] * 0.5);
  cp = math.cos(yaw_pitch_roll[1] * 0.5);
  sp = math.sin(yaw_pitch_roll[1] * 0.5);

  return [cy * sr * cp - sy * cr * sp,
  cy * cr * sp + sy * sr * cp,
  sy * cr * cp - cy * sr * sp,
  cy * cr * cp + sy * sr * sp]
    
def getButtonMap(buttons, axisX, axisY):
  map = {"system":False, "application_menu":False, "trackpad_click":False, "trackpad_touch":False,  "grip":False, "trigger":False, "dpad_left":False, "dpad_up":False, "dpad_right":False, "dpad_down":False}
  map["system"] = buttons == 8
  map["application_menu"] = buttons == 4
  map["trackpad_touch"] = buttons == 32
  map["trackpad_click"] = buttons == 33
  map["grip"] = buttons == 16
  map["trigger"] = buttons == 2  
  if axisX > 0.3 and map["trackpad_click"]:
    map["dpad_right"] = True
    map["trackpad_click"] = True
    
  if axisX < -0.3 and map["trackpad_click"]:
    map["dpad_left"] = True
    map["trackpad_click"] = True
    
  if axisY > 0.3  and map["trackpad_click"]:
    map["dpad_up"] = True
    map["trackpad_click"] = True
    
  if axisY < -0.3  and map["trackpad_click"]:
    map["dpad_down"] = True  
    map["trackpad_click"] = True  
  return map

if starting:
  offset = [0.0, 0.0, 0.0]
  alvr.two_controllers = True
  alvr.override_head_position = True
  alvr.override_controller_position = True
  alvr.override_controller_orientation = True

# You need to set this variable, to enable position
alvr.head_position[0] = noloDataTracker.poseHmd[0]
alvr.head_position[1] = noloDataTracker.poseHmd[1]
alvr.head_position[2] = -noloDataTracker.poseHmd[2]

#diagnostics.watch(noloDataTracker.poseHmd[0])
#diagnostics.watch(noloDataTracker.poseHmd[1])
#diagnostics.watch(-noloDataTracker.poseHmd[2])

# Right Controller
alvr.controller_position[0][0] = noloDataTracker.poseRight[0]
alvr.controller_position[0][1] = noloDataTracker.poseRight[1]
alvr.controller_position[0][2] = -noloDataTracker.poseRight[2]

#diagnostics.watch(alvr.input_head_orientation[0])
#diagnostics.watch(alvr.input_head_orientation[1])
#diagnostics.watch(alvr.input_head_orientation[2])
#diagnostics.watch(alvr.input_head_orientation[3])

orientationR = quaternion2euler(noloDataTracker.quaternionRight)
alvr.controller_orientation[0][0] = orientationR[0]
alvr.controller_orientation[0][1] = -orientationR[1]
alvr.controller_orientation[0][2] = -orientationR[2]

alvr.trackpad[0][0] = noloDataTracker.controllerRightAxisX
alvr.trackpad[0][1] = noloDataTracker.controllerRightAxisY
#diagnostics.watch(noloDataTracker.controllerRightButtons)
#diagnostics.watch(noloDataTracker.controllerRightAxisX)
#diagnostics.watch(noloDataTracker.controllerRightAxisY)

right_buttons_map = getButtonMap(noloDataTracker.controllerRightButtons, noloDataTracker.controllerRightAxisX, noloDataTracker.controllerRightAxisY)
for k,v in right_buttons_map.iteritems():
  alvr.buttons[0][alvr.Id(k)] = v

if right_buttons_map["trigger"]:
  alvr.trigger[0] = 1.0
  alvr.trigger_right[0] = 1.0
else:
  alvr.trigger[0] = 0.0
  alvr.trigger_right[0] = 0.0

# Left Controller
alvr.controller_position[1][0] = noloDataTracker.poseLeft[0]
alvr.controller_position[1][1] = noloDataTracker.poseLeft[1]
alvr.controller_position[1][2] = -noloDataTracker.poseLeft[2]

orientationL = quaternion2euler(noloDataTracker.quaternionLeft)
alvr.controller_orientation[1][0] = orientationL[0]
alvr.controller_orientation[1][1] = -orientationL[1]
alvr.controller_orientation[1][2] = -orientationL[2]

alvr.trackpad[1][0] = noloDataTracker.controllerLeftAxisX
alvr.trackpad[1][1] = noloDataTracker.controllerLeftAxisY

left_buttons_map = getButtonMap(noloDataTracker.controllerLeftButtons, noloDataTracker.controllerLeftAxisX, noloDataTracker.controllerLeftAxisY)
for k,v in left_buttons_map.iteritems():
  alvr.buttons[1][alvr.Id(k)] = v


if left_buttons_map["trigger"]:
  alvr.trigger[1] = 1.0
  alvr.trigger_left[1] = 1.0
else:
  alvr.trigger[1] = 0.0
  alvr.trigger_left[1] = 0.0


if alvr.input_haptic_feedback[0][1] > 0:
  noloDataTracker.controllerRightHapticPuls = 100
else:
  noloDataTracker.controllerRightHapticPuls = 0

if alvr.input_haptic_feedback[1][1] > 0:
  noloDataTracker.controllerLeftHapticPuls = 100
else:
  noloDataTracker.controllerLeftHapticPuls = 0

