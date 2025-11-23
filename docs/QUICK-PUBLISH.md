# EasyAF Quick Publish Guide

## For Developers: Publishing Updates

### Option 1: Automated (Recommended)

```powershell
# 1. Update your code
# 2. Run the script:
.\Publish-EasyAF.ps1 -IncrementVersion
```

That's it! ?

---

### Option 2: Visual Studio GUI

1. **Open Solution** in Visual Studio
2. **Right-click** `EasyAF.Shell` project
3. **Click "Publish"**
4. **Click "Publish Now"**

Done! Users auto-update on next launch.

---

## Configuration

Edit `publish-config.json` to change deployment location:

```json
{
  "publishLocation": "\\\\yourserver\\apps\\EasyAF",
  "installUrl": "\\\\yourserver\\apps\\EasyAF"
}
```

---

## Versioning

- **Bug fix:** Increment patch (`1.2.15` ? `1.2.16`)
- **New feature:** Increment minor (`1.2.0` ? `1.3.0`)
- **Breaking change:** Increment major (`1.0.0` ? `2.0.0`)

---

## Troubleshooting

### "Path not accessible"
- Check network share permissions
- Verify path in `publish-config.json`
- Ensure you're on corporate network

### "Build failed"
- Clean solution first: `Build ? Clean Solution`
- Rebuild: `Build ? Rebuild Solution`
- Check error messages in Output window

### Users not getting updates
- Verify `installUrl` matches `publishLocation`
- Check network share is accessible
- Have users restart the app

---

## User Installation

Send this to new users:

1. Navigate to: `\\yourserver\apps\EasyAF`
2. Double-click `EasyAF.application`
3. Click **Install**

Auto-updates happen automatically on launch!

---

## Need Help?

- **Full Guide:** See `docs/DEPLOYMENT.md`
- **IT Support:** [Your IT Contact]
- **Developer:** [Your Dev Contact]

---

**Last Updated:** 2025-01-20
