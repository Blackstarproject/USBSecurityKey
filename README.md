# USBSecurityKey
A Digital Key for Your Computer 🔑
Imagine you could turn a regular USB flash drive into a physical key for your computer, just like the key to your house or car. 
That's exactly what this application does. Its main purpose is to boost your digital security by making sure that you, the authorized 
user, are physically present at your computer. If you walk away and take your key with you, the computer instantly locks itself down,
protecting your information from anyone who might walk by.

The system is split into two parts: the "Key Maker" and the "Security Guard."

The Key Maker: Forging Your Unique Key
Before you can use the system, you need to create your special key. This is done once using a simple console program called the 
ProvisioningTool.

Choosing the Drive: You plug a standard USB drive into your computer and run the tool. It shows you a list of available drives, 
and you choose the one you want to transform.

Secret Handshake Setup: The tool then performs a digital ceremony on the drive. It generates a unique, matched pair of cryptographic 
keys: a public key and a private key.

Think of the public key as a custom-made lock. You can make copies of this lock and give it to anyone; it can only be used to check if 
the correct key is being used.

The private key is the one-and-only master key that fits the lock. It's incredibly secret and powerful.

Securing the Master Key: To protect this master key, the tool asks you to create a password. It then uses this password to encrypt the
private key, scrambling it into an unreadable format.

Placing the Files: Finally, the tool places three files onto the USB drive:

public.key: The "lock."

private.key: The secret "master key," safely encrypted.

password.txt: The password needed to unlock the master key.

At the end of this process, your ordinary USB drive has been successfully provisioned. It now holds a unique digital identity.

The Security Guard: Always on Watch
The main application, UsbSecurityKey.exe, is the "Security Guard" that runs silently in your computer's background. You won't see a
window for it, just a small icon in your system tray. This guard has one critical job: to constantly check for the presence of your 
specific USB key.

The Authentication Process
When you plug in a USB drive, the Security Guard instantly wakes up and begins a two-stage verification process to see if it's the real
key.

Stage 1: The Fast Pass (Token Check)
For efficiency, the guard first looks for a file named token.txt. Think of this as a temporary security pass, like a stamp on your hand 
at an event.

If a valid, unexpired token exists, the guard knows the key was recently authenticated. It grants access and the check is complete in a
fraction of a second. This prevents the system from having to do the heavy-duty security check every few seconds.

Stage 2: The Full Handshake (RSA Challenge)
If there is no token, or if the token has expired, the guard performs the full, rigorous "secret handshake."

It reads the public key, the encrypted private key, and the password from the USB drive.

It uses the password to temporarily decrypt the private key in the computer's memory.

It creates a random, one-time challenge (like saying, "Please sign this random number: 8675309").

It uses the decrypted private key to "sign" this challenge, creating a unique digital signature.

Finally, it uses the public key (the "lock") to verify that the signature is authentic. Since only the true private key could have 
created a valid signature, this proves the USB drive is genuine.

If this full handshake is successful, the guard immediately creates a new, valid token and saves it to the USB drive, ensuring the next 
check can use the fast pass.

The Lockdown: 🔒
The most important part of the Security Guard's job is what happens when the key is removed. The application runs its check every few 
seconds and gets an immediate alert from Windows whenever a USB device is disconnected.

The moment it detects that the authenticated key is no longer present, it executes its primary command: it instantly locks your Windows 


INSTRUCTIONS: 
## Phase 1: Building the Programs from Code
First, you need to compile the source code into executable (.exe) files.

Prerequisites: Make sure you have Visual Studio installed with the .NET desktop development workload.

Open the Solution: Find the solution file (e.g., UsbSecurityKey.sln) and double-click it to open the entire project in Visual Studio.

Build the Solution: In the top menu bar of Visual Studio, click Build -> Build Solution. This will compile both the UsbSecurityKey and the ProvisioningTool projects.

Locate the Files: After the build succeeds, you'll find the executable files in their respective project folders.

The key maker is at: ProvisioningTool\bin\Debug\ProvisioningTool.exe

The security guard is at: UsbSecurityKey\bin\Debug\UsbSecurityKey.exe

## Phase 2: Creating Your Key (One-Time Setup)
This phase uses the ProvisioningTool to turn your USB drive into a security key.

Insert Your USB Drive: Plug a USB flash drive that you are okay with erasing into your computer. Back up any important data from it first!

Run the Provisioning Tool: Navigate to the ProvisioningTool\bin\Debug folder and double-click ProvisioningTool.exe. A console window will appear.

Select the Drive: The tool will list all available removable drives. Type the number corresponding to your USB drive and press Enter.

Confirm Formatting: The tool will warn you that the drive will be prepared for use. Press y to confirm.

Create a Password: You'll be prompted to enter a password. This password encrypts the secret key on the drive. Type a strong password and press Enter.

Verification: The tool will automatically perform all the necessary steps: renaming the drive, generating cryptographic keys, and saving the files. When it's finished, you'll see a "SUCCESS!" message.

Your USB drive is now a provisioned security key, containing the files public.key, private.key, and password.txt.

## Phase 3: Running the Security Application
Now you can run the main application that locks your PC.

Run the Application: Navigate to the UsbSecurityKey\bin\Debug folder and double-click UsbSecurityKey.exe.

Check the System Tray: No window will open. Instead, look in your system tray (the area by the clock in the bottom-right of your screen).
You will see a new icon.

Yellow Warning Shield: This means the application is running, but your security key is not plugged in or authenticated.

Blue "i" Icon: This means your key is plugged in and successfully authenticated. Your computer is secure.

Interact with the App: You can right-click the icon to bring up a small menu. The only option is Exit, which you can use to close the 
application completely.

## Phase 4: Your New Daily Workflow 🚶‍♂️
Using the application day-to-day is incredibly simple.

When you're working: Keep the provisioned USB key plugged into your computer. The application will continuously verify it's there, 
and your computer will function normally.

When you leave your desk: Simply unplug the USB drive. Your workstation will instantly lock, preventing anyone from accessing it.

When you return: Plug the USB drive back in. Then, log into Windows using your normal password or PIN. The security application will 
automatically re-authenticate your key in the background.

### Pro Tip: Start Automatically with Windows
To avoid having to manually start the application every day, you can make it launch automatically when you log in.

Press Windows Key + R to open the Run dialog.

Type shell:startup and press Enter. This will open your personal Startup folder.

Go back to the UsbSecurityKey\bin\Debug folder.

Right-click on UsbSecurityKey.exe and select Create shortcut.

Cut or copy this new shortcut and paste it into the Startup folder you opened in step 2.

Now, the security application will start automatically every time you log into Windows.
workstation. This is the exact same effect as pressing Windows Key + L. Your session is secured, and your password is required to get 
back in, preventing any unauthorized access while you're away from your desk. When you return, you simply plug the key back in, 
and the system is ready for you to unlock it.
