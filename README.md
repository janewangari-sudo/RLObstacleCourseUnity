# RL Obstacle Course Navigator – AgentSphere (Unity ML-Agents)

## 1. Introduction & Overview

This project, developed for a Reinforcement Learning course, features an  agent ("AgentSphere") trained using the Unity ML-Agents Toolkit. 
The primary objective for AgentSphere is to navigate a complex, dynamic, and procedurally generated 3D obstacle course to reach a moving target. 
Key aspects of this project include sophisticated environment design with multiple dynamic elements, advanced agent perception using raycasts and vector observations,nuanced reward shaping, 
and a systematic approach to hyperparameter tuning, culminating in an agent that successfully masters its challenging environment.

This project was developed by **Jane Gathongo**.
GitHub Repository: [https://github.com/janewangari-sudo/RLObstacleCourseUnity](https://github.com/janewangari-sudo/RLObstacleCourseUnity)

## 🎮 Gameplay Overview

- **Agent**: White sphere (`AgentSphere`)
- **Goal**: Reach the green target (`TargetSphere`)
- **Environment**:
  - Procedural static obstacles (1–2 per episode)
  - One moving pillar obstacle with sinusoidal movement
  - Randomized target position after every success
- **Rewards**:
  - ✅ Reaching Target: `+1.2`
  - ❌ Collision with Wall/Obstacle: `-0.1`
  - ⏱️ Time penalty per step: `-0.001`
  - ↘️ Proximity bonus: `+ (distance delta * 0.01)`
  - 🚫 Wrong direction penalty: `-0.02`
  - 💀 Fall off platform: `-1.0` and episode end

---

## 2. Key Features

This project successfully implements and demonstrates concepts in Reinforcement Learning:

* **3D Environment:** The entire simulation environment and agent interactions are fully 3D, built within the Unity game engine.
* **Complex Environment:** The environment features a high degree of complexity through a combination of manually placed static obstacles, a primary moving obstacle with randomized behavior ("MovingPillar1"), and procedurally generated static obstacles that alter the course layout each episode. The target also dynamically repositions.
* **Parallel Environment):** The final successful training process leveraged multiple parallel environment instances, significantly accelerating data collection and training speed.
* **Moving/Random Components:**
    * The **target** moves to a new random (and valid) location after being successfully reached.
    * The **"MovingPillar1"** obstacle navigates along a randomly chosen X or Z axis with sinusoidal motion each episode.
    * Additional static obstacles are **procedurally spawned** in random valid locations at the start of each episode.
* **Sensors:** The agent perceives its environment using:
    * A `RayPerceptionSensor3D` configured with `[9]` rays, a `[120]` degree field of view, and an increased `Ray Length` of `[35]` units to detect "Obstacle", "Wall", and "Target" tagged GameObjects.
    * Vector observations (5 values): agent's XZ velocity, normalized XZ direction to target, and direct distance to target.
* **Continuous Actions:** The agent utilizes a 2D continuous action space, outputting values (typically -1 to +1) for forces applied along the X and Z axes.
* **Using TensorBoard:** TensorBoard was used extensively throughout the project to monitor training progress in real-time, compare the performance of different hyperparameter configurations, and visualize key metrics like Mean Cumulative Reward and Episode Length, leading to the final successful model.
* **Using own configuration YAML file:** Multiple custom YAML configuration files (e.g., `Config_With_Curiosity2.yaml`) were created and iterated upon to define agent hyperparameters, network architectures, and reward signal configurations.
* **Trying out multiple hyperparameter combinations, compare results in TensorBoard:** A systematic approach to experimentation was employed, tuning parameters such as learning rate, network size (`hidden_units`), exploration (`beta`), curiosity module strength, and critical reward function components (target reward magnitude, obstacle collision penalties). The impact of these changes was analyzed in TensorBoard to guide subsequent experiments towards the successful final configuration.

---

## 3. Environment Details

The training environment is a walled arena (approximately 50x50 units) meticulously designed to challenge the agent:
* **Agent Starting Position:** A fixed `Vector3` position where the `AgentSphere` begins each episode, marked by an "AgentStartMarker" GameObject.
* **Target (TargetSphere):** The goal. A trigger object controlled by `TargetMovement.cs`, which repositions it to a new random valid location (avoiding obstacles and the agent's start) each time the agent successfully reaches it.
* **Fixed Static Obstacles:** A set of manually placed obstacles ("Obstacle1", "Obstacle2", "Obstacle3") providing consistent structural challenges.
* **Moving Pillar ("MovingPillar1"):** A tall cylindrical obstacle controlled by `MovingObstacle.cs`. At the start of each episode, it randomly chooses to move along either the world X or Z axis with smooth sinusoidal motion. Its speed and movement distance are configurable.
* **Procedural Static Obstacles:** Governed by `AgentSphereAgent.cs`. At the start of each new episode, `[1]` to `[2]` additional static obstacles are instantiated from a "SpawnableObstacle_Static" prefab. The spawning logic includes checks to prevent overlaps with the agent's starting position, other newly spawned obstacles, and the fixed/moving obstacles.
* **Boundary Walls:** Enclose the arena and are tagged "Wall".

---

## 4. Agent Details: `AgentSphereAgent.cs`

The `AgentSphereAgent` is a sphere with a `Rigidbody` component, learning through interaction.

* **Observations:**
    * **Vector (5 values):** X-velocity, Z-velocity, normalized X-direction to target, normalized Z-direction to target, distance to target.
    * **RayPerceptionSensor3D:** Configured with `[8]` rays, `[120]` degrees FoV, and `Ray Length: [35]` units. Detects "Obstacle", "Wall", "Target".
* **Actions (Continuous, 2D):**
    * Action 0: Force along X-axis (scaled by `moveForce`).
    * Action 1: Force along Z-axis (scaled by `moveForce`).
    * The `moveForce` was tuned in Inspector to `[5f]`.
* **Key Reward Function Components (from `AgentSphere.cs` that produced the best results):**
    * Reaching Target: `+1.2f` & `EndEpisode()`.
    * Collision with "Obstacle" or "Wall": `AddReward(-0.4f);` (Non-terminal).
    * Proximity Reward: `AddReward(distanceDelta * 0.01f);`.
    * Wrong Direction Penalty: `AddReward(-0.02f);` if `Vector3.Dot() < -0.5f`.
    * Per-Step Penalty: `AddReward(-0.001f);`.
    * Fell Off Platform: `AddReward(-1.0f); EndEpisode();` (if `transform.localPosition.y < -1f`).

---

## 5. Software & Frameworks Used

* **Unity Editor:** Version `2022.3.62f1`
* **Unity ML-Agents Toolkit:**
    * Unity Package Version: `2.0.1` (Release 19)
    * Python Package `ml-agents`: `0.30.0`
    * Python Package `ml-agents-envs`: `0.30.0`
    * Communicator API: `1.5.0`
* **Python:** Version `3.9 via Anaconda/Miniconda`
* **PyTorch:** Version `1.7.1+cpu` 

---

## 🧪 Hyperparameter Experiments & Training (Guideline #10)

Several training experiments were conducted to find an optimal configuration. The agent was trained on the full dynamic environment. Key configurations and their outcomes are summarized below:

### 🧪 Hyperparameter Experiments & Key Learnings (Guideline #10)

A series of experiments were conducted to optimize the `AgentSphereBehavior`. The following table highlights key configurations tested on the full dynamic environment (moving target, random moving pillar, and procedural static obstacles), using the final agent script (Target Reward: +5.0, Obstacle Collision: -0.1 non-terminal, `moveForce`: `[YourTunedValue, e.g., 6f]`, etc.) unless otherwise noted in "Key Differences & Notes".

| Run Name (TensorBoard Label)                                  | Color | Hidden Units (Policy) | Batch Size | Learning Rate (Policy) | Curiosity Strength (YAML) | Beta (YAML) | Result (Peak/Stable Mean Reward) | Key Differences & Notes                                                                                                |
| :------------------------------------------------------------ | :---: | :-------------------- | :--------- | :--------------------- | :------------------------ | :---------- | :----------------------------- | :--------------------------------------------------------------------------------------------------------------------- |
| 🟠 `Exp_With_CuriosityAgentSphereBehavior`                    |  🟠   | 128 (2 layers)        | 128        | 0.0001                 | 0.05                      | 0.02        | **~ +1.8 ⭐ Best** | **Champion Run.** Achieved high positive rewards with excellent stability and efficiency. Showcased strong learning. |
| 🔵 `Exp_With_No_CuriosityAgentSphereBehavior`                 |  🔵   | 128 (2 layers)        | 128        | 0.0001                 | **None** | 0.02        | ~ -0.2                         | Compared to Champion, removing curiosity significantly reduced performance. Curiosity appears vital.            |
| 🩷 `Exp_With_Curiosity_EndEpisodeOnHittingObstacleAgentSphereBehavior` |  🩷   | 128 (2 layers)        | 128        | 0.0001                 | 0.05                      | 0.02        | `[e.g., ~ -0.7]`               | Obstacle collision was terminal (`EndEpisode()`) & likely a harsher penalty (e.g., -1.0). Performed worse than champion. |
| 💚 `[Your Run ID for earlier -1.1 Plateau, e.g., Exp_Curiosity_s0.02]` |  💚   | 128 (2 layers)        | 128        | 0.0001                 | 0.02 (Lower than champ)   | 0.02        | ~ -1.1                         | Very stable, but plateaued. Increasing curiosity strength to 0.05 (as in champion) was beneficial.    |
| ⚪️ `[Example: Exp1_Net256 - if you ran a comparable version]` |  ⚪️   | `256 (2 layers)`      | `128`      | `0.0001`               | `0.05`                    | `0.02`      | `[Your Result]`                | `[e.g., Did not improve over 128HU, or slower convergence]`                                             |

*This table demonstrates the iterative tuning process. The combination of a +5.0 target reward, non-terminal soft obstacle penalties (-0.1), specific `beta` (0.02) and `curiosity strength` (0.05) values, a low `learning_rate` (0.0001), and a `128x2` network proved to be the most effective configuration for this complex task.*
### Best Performing Configuration: `Config_With_Curiosity2.yaml` 

The configuration that produced the best results (Mean Reward consistently **~+1.377** or your highest achieved stable positive value) is detailed in the `Config_With_Curiosity2.yaml` file and the final version of the `AgentSphere.cs` script (as provided in the repository). The critical factors for success included:
* A significantly attractive **Target Reward of +1.2f**.
* A **softened, non-terminal penalty for obstacle/wall collisions (-0.4f)**, allowing the agent to learn from minor mistakes.
* **Enhanced agent perception** through increased raycast length (`[35]` units).
* Robust **exploration mechanisms:** `beta: 0.02` and `curiosity: strength: 0.05` in the YAML.
* A **stable learning rate:** `learning_rate: 0.0001`.
* An appropriate **network size:** `hidden_units: 128`, `num_layers: 2` for the main policy.
* **Specific reward shaping** in `AgentSphere.cs`: including proximity rewards and a "wrong direction" penalty.
* A **tuned `moveForce`** (`5f`) in the Unity Inspector for the `AgentSphereAgent` for precise control.

---

## 📊 TensorBoard Visualizations (Guideline #8)

The following TensorBoard screenshots illustrate the learning progress of the champion experimental run.

### Champion Training Run: `[YourChampionRunID, e.g., Exp_LongRays_FinalPush]`
*(This graph should clearly show the Mean Cumulative Reward climbing to your best positive value, and the Episode Length decreasing and stabilizing.)*

**[EMBED YOUR BEST TENSORBOARD SCREENSHOT HERE - The one showing the +4.377 (or better) curve for Cumulative Reward and Episode Length. Ensure this image is in your GitHub repo, e.g., in a `docs/images/` folder.]**
*Markdown Example: `![Champion Run: Cumulative Reward & Episode Length](docs/images/ChampionRun_TensorBoard.png)`*

---
## 📷 Screenshots

> Upload images to your repo in an `/assets/` folder and update the links below.

| Agent Navigating | Training Graphs | Arena Setup |
|------------------|-----------------|-------------|
| ![Gameplay](assets/agent-running.gif) | ![Rewards](assets/tensorboard-rewards.png) | ![Arena](assets/arena-layout.png) |

---
## 📺 Watch the Demo

[![Watch the demo](https://img.youtube.com/vi/YOUTUBE_VIDEO_ID/0.jpg)](https://youtu.be/YOUTUBE_VIDEO_ID)

> Replace `YOUTUBE_VIDEO_ID` with your actual YouTube video ID.

---
## 🚀 How to Set Up and Run

### a. Prerequisites
* Unity Hub & Unity Editor `2022.3.62f1`
* Anaconda/Miniconda (Python `3.9`)
* Git

### b. Clone Repository
```bash
git clone [https://github.com/janewangari-sudo/RLObstacleCourseUnity.git](https://github.com/janewangari-sudo/RLObstacleCourseUnity.git)
cd RLObstacleCourseUnity
