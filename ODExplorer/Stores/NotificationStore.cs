using EliteJournalReader.Events;
using Microsoft.EntityFrameworkCore.Migrations;
using NAudio.Utils;
using NetTopologySuite.Utilities;
using Newtonsoft.Json.Linq;
using ODExplorer.Models;
using ODExplorer.Notifications;
using ODUtils.Extensions;
using ODUtils.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;
using ToastNotifications;
using ToastNotifications.Core;
using ToastNotifications.Lifetime;
using ToastNotifications.Position;
using Application = System.Windows.Application;

namespace ODExplorer.Stores
{
    public sealed class NotificationStore
    {
        public NotificationStore(SettingsStore settingsStore)
        {
            this.settingsStore = settingsStore;
            var settings = settingsStore.NotificationSettings;
            notifier = new Notifier(cfg =>
            {
                cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(TimeSpan.FromSeconds(settings.DisplayTime), MaximumNotificationCount.FromCount(settings.MaxNotificationCount));
                cfg.PositionProvider = new PrimaryScreenPositionProvider(settings.DisplayRegion, settings.XOffset, settings.YOffset);
                cfg.DisplayOptions.Width = GetNotificationWidth(settings.Size);
                cfg.DisplayOptions.TopMost = true;
            });

            messageOptions = new MessageOptions()
            {
                FontSize = GetFontSize(settings.Size),
                FreezeOnMouseEnter = true,
                UnfreezeOnMouseLeave = true,
                ShowCloseButton = false,
                CloseClickAction = OnNotificationClose,
                NotificationClickAction = OnNotificationClick,
                Tag = string.Empty,
            };
        }

        private Notifier notifier;
        private MessageOptions messageOptions;
        private readonly SettingsStore settingsStore;

        private void OnNotificationClick(NotificationBase notificationBase)
        {
            if (notificationBase is TestNotification)
            {
                ODUtils.Helpers.OperatingSystem.OpenUrl("https://github.com/WarmedxMints/OD-Explorer");
                return;
            }
            notificationBase.Close();
        }

        private void OnNotificationClose(NotificationBase notificationBase)
        {
            //TO DO
        }

        internal void ChangeNotifierSetting(NotificationSettings settings)
        {
            notifier?.Dispose();

            notifier = new Notifier(cfg =>
            {
                cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(TimeSpan.FromSeconds(settings.DisplayTime), MaximumNotificationCount.FromCount(settings.MaxNotificationCount));
                cfg.PositionProvider = new PrimaryScreenPositionProvider(settings.DisplayRegion, settings.XOffset, settings.YOffset);
                cfg.DisplayOptions.Width = GetNotificationWidth(settings.Size);
                cfg.DisplayOptions.TopMost = true;

            });

            messageOptions = new MessageOptions()
            {
                FontSize = GetFontSize(settings.Size),
                FreezeOnMouseEnter = true,
                UnfreezeOnMouseLeave = true,
                ShowCloseButton = false,
                CloseClickAction = OnNotificationClose,
                NotificationClickAction = OnNotificationClick,
                Tag = string.Empty,
            };

            if (settingsStore.NotificationSettings.NotificationsEnabled)
                ShowTestNotification();
        }

        internal void ShowTestNotification()
        {
            notifier.Notify(() => new TestNotification(string.Empty, messageOptions, settingsStore.NotificationSettings));
        }

        internal void ShowWorthMappingNotification(SystemBody body)
        {
            if (settingsStore.NotificationSettings.NotificationsEnabled == false)
                return;

            notifier.Notify(() => new WorthMappingNotification(body, settingsStore.NotificationSettings, messageOptions));
        }

        internal void ShowExoBioNotification(OrganicScanItem item, string header)
        {
            if (settingsStore.NotificationSettings.NotificationsEnabled == false)
                return;

            notifier.Notify(() => new ExoBioNotification(item, header, settingsStore.NotificationSettings, messageOptions));
        }

        internal void ShowHighValueExoBodyNotification(string bodyName, string value, string bioCount)
        {
            if (settingsStore.NotificationSettings.NotificationsEnabled == false)
                return;

            notifier.Notify(() => new ExoValuableBodyNotification(settingsStore.NotificationSettings, messageOptions, bodyName.ToUpper(), value, bioCount, "Valuable Exobiology Body"));
        }

        internal void ShowNewCodexEntriesNotification(string bodyName, Dictionary<string, bool> entries, string? currentSystemRegion)
        {
            if (settingsStore.NotificationSettings.NotificationsEnabled == false)
                return;

            notifier.Notify(() => new NewCodexEntriesNotification(settingsStore.NotificationSettings, messageOptions, bodyName.ToUpper(), entries, "Possible New Personal Codex", currentSystemRegion));
        }

        internal void ShowNewSpeciesEntriesNotification(string bodyName, Dictionary<string, bool> entries, string? currentSystemRegion)
        {
            if (settingsStore.NotificationSettings.NotificationsEnabled == false)
                return;

            notifier.Notify(() => new NewCodexEntriesNotification(settingsStore.NotificationSettings, messageOptions, bodyName.ToUpper(), entries, "Possible New Species Codex", currentSystemRegion));
        }

        internal void CopyToClipBoard(string message)
        {
            if (settingsStore.NotificationSettings.NotificationsEnabled == false)
                return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                if (ODUtils.Helpers.ClipboardHelper.SetStringToClipboard(message) == false)
                {
                    return;
                }

                if (settingsStore.NotificationOptions.HasFlag(NotificationOptions.CopyToClipboard))
                {
                    notifier.Notify(() => new CopyToClipboardNotification(settingsStore.NotificationSettings, message, messageOptions));
                }
            });
        }

        internal void FleetCarrierNotification(string message)
        {
            if (settingsStore.NotificationSettings.NotificationsEnabled == false)
                return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                notifier.Notify(() => new FleetCarrierJumpNotification(settingsStore.NotificationSettings, message, messageOptions));
            });
        }

        internal void EDSMValuableBodiesNotification(StarSystem system)
        {
            if (settingsStore.NotificationSettings.NotificationsEnabled == false)
                return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                notifier.Notify(() => new EDSMValuableBodiesNotification(system, settingsStore.NotificationSettings, string.Empty, messageOptions));
            });
        }

        internal void CheckForNotableNotifications(SystemBody body)
        {
            if (settingsStore.NotificationSettings.NotificationsEnabled == false)
                return;

            var settings = settingsStore.NotableSettings;

            if (settings.BodyNotifications.HasFlag(BodyNotification.DiverseLife)
                && body.BiologicalSignals >= settings.DiverseLifeLimit)
            {
                notifier.Notify(() => new ExoValuableBodyNotification(settingsStore.NotificationSettings, messageOptions,
                                                                      body.BodyName.ToUpper(), string.Empty,
                                                                      $"{body.BiologicalSignals} Signals",
                                                                      "Diverse Exobiology Body"));
            }

            if (settings.BodyNotifications.HasFlag(BodyNotification.SmallPlanet) && body.Radius <= settings.SmallRadius)
            {
                var radius = settingsStore.SystemGridSetting.DistanceUnit switch
                {
                    Distance.Miles => $"{body.Radius * 0.62137:N0} mi",
                    _ => $"{body.Radius:N0} km"
                };

                notifier.Notify(() => new NotableBodyNotification(BodyNotification.SmallPlanet, body,
                                                                  "Small Radius Body",
                                                                  radius,
                                                                  settingsStore.NotificationSettings, messageOptions));
            }

            if (settings.BodyNotifications.HasFlag(BodyNotification.HighEccentricity) && body.Eccentricity >= settings.EccentricityMin)
            {

                notifier.Notify(() => new NotableBodyNotification(BodyNotification.HighEccentricity, body,
                                                                  "High Eccentricity Body",
                                                                  $"Eccentricity: {body.Eccentricity:N4}",
                                                                  settingsStore.NotificationSettings, messageOptions));
            }

            if (settings.BodyNotifications.HasFlag(BodyNotification.NestedMoon)
               && body.Parents?.Count > 1
               && body.Parents[0].Type == EliteJournalReader.Events.ParentType.Planet
               && body.Parents[1].Type == EliteJournalReader.Events.ParentType.Planet)
            {
                notifier.Notify(() => new NotableBodyNotification(BodyNotification.NestedMoon, body,
                                                                 "Nested Moon",
                                                                 string.Empty,
                                                                 settingsStore.NotificationSettings, messageOptions));
            }

            if (settings.BodyNotifications.HasFlag(BodyNotification.FastRotation)
               && body.TidalLock == false
               && Math.Abs(body.RotationPeriod * 24) <= settings.FastRotationMin)
            {
                notifier.Notify(() => new NotableBodyNotification(BodyNotification.FastRotation, body,
                                                                               "Fast Rotating Body",
                                                                               $"Period: {Math.Abs(body.RotationPeriod * 24):N1} hours",
                                                                               settingsStore.NotificationSettings, messageOptions));
            }

            if (settings.BodyNotifications.HasFlag(BodyNotification.FastOrbit)
                && Math.Abs(body.OrbitalPeriod * 24) <= settings.FastOrbit)
            {
                notifier.Notify(() => new NotableBodyNotification(BodyNotification.FastOrbit, body,
                                                                               "Body With Fast Orbit",
                                                                               $"Period: {Math.Abs(body.OrbitalPeriod * 24):N1} hours",
                                                                               settingsStore.NotificationSettings, messageOptions));
            }

            if (settings.BodyNotifications.HasFlag(BodyNotification.WideRings)
               && body.Rings?.Count > 0)
            {
                var rings = body.Rings.Where(x => !x.Name.Contains("Belt"));

                foreach (var ring in rings)
                {
                    var ringWidth = ring.OuterRad - ring.InnerRad;

                    if (ringWidth > body.Radius * 1000 * settings.RingWidthRadiusMultiplier)
                    {
                        var ringName = ring.Name.Replace(body.BodyName, "").Trim();

                        var widthLs = $"{ringWidth * 3.3356e-9:N2} ls";
                        var widthUser = settingsStore.SystemGridSetting.DistanceUnit switch
                            {
                                Distance.Miles => $"{ringWidth * 0.00062137:N0} mi",
                                _ => $"{ringWidth / 1000:N0} km"
                            };

                        var radius = settingsStore.SystemGridSetting.DistanceUnit switch
                        {
                            Distance.Miles => $"{body.Radius * 0.62137:N0} mi",
                            _ => $"{body.Radius:N0} km"
                        };

                        notifier.Notify(() => new NotableBodyNotification(BodyNotification.WideRings, body,
                                                                          "Body With Wide Ring",
                                                                          $"{ringName}\nWidth: {widthLs} | {widthUser}\nParent Radius: {radius}",
                                                                          settingsStore.NotificationSettings, messageOptions));
                    }
                }
            }

            if(settings.BodyNotifications.HasFlag(BodyNotification.ShepherdMoon) 
                && body.Parents is not null && body.Parents?.Count > 0 
                && body.Parents?.First().Type != ParentType.Null)
            {
                var parent = GetParent(body);

                var parentRings = parent?.Rings?.Where(x => !x.Name.Contains("Belt"))?.LastOrDefault();

                if (parentRings != null && parentRings?.OuterRad > body.SemiMajorAxis)
                {
                    var ringRadius = parentRings?.OuterRad;
                    var ringRadiusLs = $"{ringRadius * 3.3356e-9:N2} ls";
                    var ringRadiusUser = settingsStore.SystemGridSetting.DistanceUnit switch
                    {
                        Distance.Miles => $"{ringRadius * 0.00062137:N0} mi",
                        _ => $"{ringRadius / 1000:N0} km"
                    };

                    var bodyOrbitLs = $"{body.SemiMajorAxis * 3.3356e-9:N2} ls";
                    var bodyOrbitUser = settingsStore.SystemGridSetting.DistanceUnit switch
                    {
                        Distance.Miles => $"{body.SemiMajorAxis * 0.00062137:N0} mi",
                        _ => $"{body.SemiMajorAxis / 1000:N0} km"
                    };

                    notifier.Notify(() => new NotableBodyNotification(BodyNotification.ShepherdMoon, body,
                                                                          "Shepherd Moon",
                                                                          $"Moon Orbit: {bodyOrbitLs} | {bodyOrbitUser}\nRing Radius: {ringRadiusLs} | {ringRadiusUser}",
                                                                          settingsStore.NotificationSettings, messageOptions));
                }
            }

            if (body.Landable == false)
            {
                return;
            }

            if (settings.BodyNotifications.HasFlag(BodyNotification.LandableTerraformable) && body.Terraformable)
            {
                notifier.Notify(() => new NotableBodyNotification(BodyNotification.LandableTerraformable, body,
                                                                  "Landable Terraformable Body",
                                                                  body.TerraformState.GetEnumDescription(),
                                                                  settingsStore.NotificationSettings, messageOptions));
            }

            if (settings.BodyNotifications.HasFlag(BodyNotification.LandableWithRings) && body.Rings?.Count > 0)
            {
                notifier.Notify(() => new NotableBodyNotification(BodyNotification.LandableWithRings, body,
                                                                  "Landable Body With Rings",
                                                                  string.Empty,
                                                                  settingsStore.NotificationSettings, messageOptions));
            }

            if (settings.BodyNotifications.HasFlag(BodyNotification.LandableHighGravity) && body.SurfaceGravity >= settings.HighSurfaceGravity)
            {
                notifier.Notify(() => new NotableBodyNotification(BodyNotification.LandableHighGravity, body,
                                                                  "Landable High Gravity Body",
                                                                  $"{body.SurfaceGravity:N2} g",
                                                                  settingsStore.NotificationSettings, messageOptions));
            }

            if (settings.BodyNotifications.HasFlag(BodyNotification.LandableLargeRadius) && body.Radius >= settings.LargeRadius)
            {
                var radius = settingsStore.SystemGridSetting.DistanceUnit switch
                {
                    Distance.Miles => $"{body.Radius * 0.62137:N0} mi",
                    _ => $"{body.Radius:N0} km"
                };

                notifier.Notify(() => new NotableBodyNotification(BodyNotification.LandableLargeRadius, body,
                                                                  "Landable Large Radius Body",
                                                                  radius,
                                                                  settingsStore.NotificationSettings, messageOptions));
            }

            if (settings.BodyNotifications.HasFlag(BodyNotification.BioSignals) && body.BiologicalSignals > 0)
            {
                var message = body.BiologicalSignals > 0 ? "Signals" : "Signal";

                notifier.Notify(() => new NotableBodyNotification(BodyNotification.BioSignals, body,
                                                            "Body with Biology Signals",
                                                            $"{body.BiologicalSignals} Biological {message}",
                                                            settingsStore.NotificationSettings, messageOptions));
            }

            if (settings.BodyNotifications.HasFlag(BodyNotification.GeoSignals) && body.GeologicalSignals > 0)
            {
                var message = body.GeologicalSignals > 0 ? "Signals" : "Signal";

                notifier.Notify(() => new NotableBodyNotification(BodyNotification.GeoSignals, body,
                                                            "Body with Geological Signals",
                                                            $"{body.GeologicalSignals} Geological {message}",
                                                            settingsStore.NotificationSettings, messageOptions));
            }
        }

        #region Notification Helpers
        private static SystemBody? GetParent(SystemBody body)
        {
            var parentId = body.Parents?.First().BodyID;

            if (parentId == null)
            {
                return null;                
            }

            var parent = body.Owner.SystemBodies.FirstOrDefault(b => b.BodyID == parentId);

            if (parent == null)
            {
                return null;
            }
            return parent;
        }

        private static double GetNotificationWidth(NotificationSize size)
        {
            return size switch
            {
                NotificationSize.Medium => 500,
                NotificationSize.Large => 750,
                _ => 350
            };
        }

        private static double GetFontSize(NotificationSize size)
        {
            return size switch
            {
                NotificationSize.Medium => 20,
                NotificationSize.Large => 28,
                _ => 14
            };
        }

        internal void Dispose()
        {
            notifier.Dispose();
        }
        #endregion
    }
}
