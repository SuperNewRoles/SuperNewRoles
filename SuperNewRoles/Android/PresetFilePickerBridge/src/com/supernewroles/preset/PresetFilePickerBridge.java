package com.supernewroles.preset;

import android.app.Activity;
import android.app.Fragment;
import android.app.FragmentManager;
import android.app.FragmentTransaction;
import android.content.ContentResolver;
import android.content.Intent;
import android.net.Uri;
import android.os.Bundle;

import com.unity3d.player.UnityPlayer;

import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.InputStream;
import java.io.OutputStream;

public final class PresetFilePickerBridge {
    private static final String FRAGMENT_TAG = "SuperNewRolesPresetFilePickerFragment";

    private PresetFilePickerBridge() {
    }

    public static void exportPreset(
            final Activity activity,
            final String requestId,
            final String sourceFilePath,
            final String suggestedName,
            final String receiverObjectName) {
        if (activity == null) {
            send(receiverObjectName, requestId, "export", "error", sourceFilePath, "Activity is null.");
            return;
        }

        activity.runOnUiThread(new Runnable() {
            @Override
            public void run() {
                PresetFilePickerFragment fragment = null;
                try {
                    fragment = PresetFilePickerFragment.create(
                            requestId,
                            "export",
                            sourceFilePath,
                            suggestedName,
                            receiverObjectName);
                    addFragment(activity, fragment, requestId);
                    fragment.startExport();
                } catch (Exception ex) {
                    if (fragment != null) {
                        fragment.removeSelf();
                    }
                    send(receiverObjectName, requestId, "export", "error", sourceFilePath, ex.toString());
                }
            }
        });
    }

    public static void importPreset(
            final Activity activity,
            final String requestId,
            final String targetFilePath,
            final String receiverObjectName) {
        if (activity == null) {
            send(receiverObjectName, requestId, "import", "error", targetFilePath, "Activity is null.");
            return;
        }

        activity.runOnUiThread(new Runnable() {
            @Override
            public void run() {
                PresetFilePickerFragment fragment = null;
                try {
                    fragment = PresetFilePickerFragment.create(
                            requestId,
                            "import",
                            targetFilePath,
                            null,
                            receiverObjectName);
                    addFragment(activity, fragment, requestId);
                    fragment.startImport();
                } catch (Exception ex) {
                    if (fragment != null) {
                        fragment.removeSelf();
                    }
                    send(receiverObjectName, requestId, "import", "error", targetFilePath, ex.toString());
                }
            }
        });
    }

    private static void addFragment(Activity activity, PresetFilePickerFragment fragment, String requestId) {
        FragmentManager fragmentManager = activity.getFragmentManager();
        String tag = FRAGMENT_TAG + ":" + requestId;
        Fragment oldFragment = fragmentManager.findFragmentByTag(tag);
        if (oldFragment != null) {
            throw new IllegalStateException("Preset file picker request is already open.");
        }

        FragmentTransaction transaction = fragmentManager.beginTransaction();
        transaction.add(fragment, tag);
        transaction.commitAllowingStateLoss();
        fragmentManager.executePendingTransactions();
    }

    static void send(
            String receiverObjectName,
            String requestId,
            String action,
            String status,
            String filePath,
            String error) {
        String payload = "{"
                + "\"requestId\":\"" + escape(requestId) + "\","
                + "\"action\":\"" + escape(action) + "\","
                + "\"status\":\"" + escape(status) + "\","
                + "\"filePath\":\"" + escape(filePath) + "\","
                + "\"error\":\"" + escape(error) + "\""
                + "}";
        UnityPlayer.UnitySendMessage(receiverObjectName, "OnPresetFilePickerResult", payload);
    }

    private static String escape(String value) {
        if (value == null) {
            return "";
        }

        StringBuilder builder = new StringBuilder(value.length() + 16);
        for (int i = 0; i < value.length(); i++) {
            char c = value.charAt(i);
            switch (c) {
                case '"':
                    builder.append("\\\"");
                    break;
                case '\\':
                    builder.append("\\\\");
                    break;
                case '\b':
                    builder.append("\\b");
                    break;
                case '\f':
                    builder.append("\\f");
                    break;
                case '\n':
                    builder.append("\\n");
                    break;
                case '\r':
                    builder.append("\\r");
                    break;
                case '\t':
                    builder.append("\\t");
                    break;
                default:
                    if (c < 0x20) {
                        String hex = Integer.toHexString(c);
                        builder.append("\\u");
                        for (int j = hex.length(); j < 4; j++) {
                            builder.append('0');
                        }
                        builder.append(hex);
                    } else {
                        builder.append(c);
                    }
                    break;
            }
        }
        return builder.toString();
    }

    public static final class PresetFilePickerFragment extends Fragment {
        private static final int REQUEST_EXPORT = 0x4e51;
        private static final int REQUEST_IMPORT = 0x4e52;
        private static final long MAX_COPY_BYTES = 32L * 1024L * 1024L;
        private String requestId;
        private String action;
        private String filePath;
        private String suggestedName;
        private String receiverObjectName;

        static PresetFilePickerFragment create(
                String requestId,
                String action,
                String filePath,
                String suggestedName,
                String receiverObjectName) {
            PresetFilePickerFragment fragment = new PresetFilePickerFragment();
            Bundle args = new Bundle();
            args.putString("requestId", requestId);
            args.putString("action", action);
            args.putString("filePath", filePath);
            args.putString("suggestedName", suggestedName);
            args.putString("receiverObjectName", receiverObjectName);
            fragment.setArguments(args);
            return fragment;
        }

        @Override
        public void onCreate(Bundle savedInstanceState) {
            super.onCreate(savedInstanceState);
            Bundle args = getArguments();
            requestId = args.getString("requestId");
            action = args.getString("action");
            filePath = args.getString("filePath");
            suggestedName = args.getString("suggestedName");
            receiverObjectName = args.getString("receiverObjectName");
        }

        void startExport() {
            Intent intent = new Intent(Intent.ACTION_CREATE_DOCUMENT);
            intent.addCategory(Intent.CATEGORY_OPENABLE);
            intent.setType("application/zip");
            intent.putExtra(Intent.EXTRA_TITLE, suggestedName == null ? "preset.snrpresets" : suggestedName);
            startActivityForResult(intent, REQUEST_EXPORT);
        }

        void startImport() {
            Intent intent = new Intent(Intent.ACTION_OPEN_DOCUMENT);
            intent.addCategory(Intent.CATEGORY_OPENABLE);
            intent.setType("*/*");
            intent.putExtra(Intent.EXTRA_MIME_TYPES, new String[]{"application/zip", "application/x-zip-compressed", "application/octet-stream"});
            startActivityForResult(intent, REQUEST_IMPORT);
        }

        @Override
        public void onActivityResult(int requestCode, int resultCode, Intent data) {
            super.onActivityResult(requestCode, resultCode, data);
            if (resultCode != Activity.RESULT_OK || data == null || data.getData() == null) {
                PresetFilePickerBridge.send(receiverObjectName, requestId, action, "cancelled", filePath, "");
                removeSelf();
                return;
            }

            if (requestCode != REQUEST_EXPORT && requestCode != REQUEST_IMPORT) {
                PresetFilePickerBridge.send(receiverObjectName, requestId, action, "error", filePath, "Unknown request code.");
                removeSelf();
                return;
            }

            startTransfer(requestCode, data.getData());
        }

        private void startTransfer(final int requestCode, final Uri uri) {
            final Activity activity = getActivity();
            if (activity == null) {
                PresetFilePickerBridge.send(receiverObjectName, requestId, action, "error", filePath, "Activity is null.");
                removeSelf();
                return;
            }

            final ContentResolver resolver = activity.getContentResolver();
            final String resultAction = requestCode == REQUEST_EXPORT ? "export" : "import";
            Thread transferThread = new Thread(new Runnable() {
                @Override
                public void run() {
                    String status = "success";
                    String error = "";
                    try {
                        if (requestCode == REQUEST_EXPORT) {
                            copyToUri(resolver, uri, filePath);
                        } else {
                            copyFromUri(resolver, uri, filePath);
                        }
                    } catch (Exception ex) {
                        status = "error";
                        error = ex.toString();
                    }
                    sendResultAndRemove(activity, resultAction, status, error);
                }
            }, "SNRPresetFilePickerCopy");
            transferThread.start();
        }

        private void sendResultAndRemove(
                Activity activity,
                final String resultAction,
                final String status,
                final String error) {
            activity.runOnUiThread(new Runnable() {
                @Override
                public void run() {
                    PresetFilePickerBridge.send(receiverObjectName, requestId, resultAction, status, filePath, error);
                    removeSelf();
                }
            });
        }

        private void copyToUri(ContentResolver resolver, Uri uri, String sourcePath) throws Exception {
            InputStream input = null;
            OutputStream output = null;
            try {
                input = new FileInputStream(sourcePath);
                output = resolver.openOutputStream(uri, "wt");
                if (output == null) {
                    throw new IllegalStateException("Could not open output stream.");
                }
                copy(input, output, MAX_COPY_BYTES);
            } finally {
                closeQuietly(input);
                closeQuietly(output);
            }
        }

        private void copyFromUri(ContentResolver resolver, Uri uri, String targetPath) throws Exception {
            InputStream input = null;
            OutputStream output = null;
            try {
                input = resolver.openInputStream(uri);
                if (input == null) {
                    throw new IllegalStateException("Could not open input stream.");
                }
                output = new FileOutputStream(targetPath, false);
                copy(input, output, MAX_COPY_BYTES);
            } finally {
                closeQuietly(input);
                closeQuietly(output);
            }
        }

        private static void copy(InputStream input, OutputStream output, long maxBytes) throws Exception {
            byte[] buffer = new byte[8192];
            long totalBytes = 0;
            int read;
            while ((read = input.read(buffer)) >= 0) {
                totalBytes += read;
                if (totalBytes > maxBytes) {
                    throw new IllegalStateException("Preset archive is too large.");
                }
                output.write(buffer, 0, read);
            }
            output.flush();
        }

        private static void closeQuietly(java.io.Closeable closeable) {
            if (closeable == null) {
                return;
            }
            try {
                closeable.close();
            } catch (Exception ignored) {
            }
        }

        private void removeSelf() {
            try {
                if (getActivity() != null) {
                    getActivity()
                            .getFragmentManager()
                            .beginTransaction()
                            .remove(this)
                            .commitAllowingStateLoss();
                }
            } catch (Exception ignored) {
            }
        }
    }
}
